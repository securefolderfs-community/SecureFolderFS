using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Ipc;

namespace SecureFolderFS.Core.FSKit.Ipc
{
    /// <summary>
    /// Production-ready Unix domain socket IPC server for FSKit service.
    /// Listens for connections and processes messages from the Bridge/Uno app.
    /// </summary>
    public sealed class FSKitIPCServer : IDisposable
    {
        private readonly string _socketPath;
        private readonly IPCRequestHandler _requestHandler;
        private Socket? _listenerSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning;
        private readonly SemaphoreSlim _startStopLock = new(1, 1);

        public bool IsRunning => _isRunning;

        public FSKitIPCServer()
        {
            // Use a short path in the user's home directory to stay under the 104 character limit
            // Unix domain socket paths have a maximum length of 104 characters on macOS
            var homePath = Environment.GetEnvironmentVariable("HOME")
                           ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var socketDir = Path.Combine(homePath, Sdk.Ipc.Constants.Sockets.SOCKET_DIRECTORY_NAME);

            // Ensure directory exists
            if (!Directory.Exists(socketDir))
            {
                try
                {
                    Directory.CreateDirectory(socketDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not create directory {socketDir}: {ex.Message}");
                }
            }

            _socketPath = Path.Combine(socketDir, "fskit.sock");
            _requestHandler = new IPCRequestHandler();

            Console.WriteLine($"FSKit IPC: Socket path configured as {_socketPath} (length: {_socketPath.Length})");
        }

        public async Task StartAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                if (_isRunning)
                {
                    Console.WriteLine("FSKit IPC: Server is already running");
                    return;
                }

                // Clean up any existing socket file
                if (File.Exists(_socketPath))
                {
                    try
                    {
                        File.Delete(_socketPath);
                        Console.WriteLine($"FSKit IPC: Deleted existing socket file at {_socketPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not delete existing socket file: {ex.Message}");
                    }
                }

                _listenerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                var endpoint = new UnixDomainSocketEndPoint(_socketPath);

                _listenerSocket.Bind(endpoint);
                _listenerSocket.Listen(10);

                _cancellationTokenSource = new CancellationTokenSource();
                _isRunning = true;

                Console.WriteLine($"FSKit IPC server listening on {_socketPath}");

                // Accept connections in a loop (fire and forget is intentional here)
                _ = Task.Run(() => AcceptConnectionsAsync(_cancellationTokenSource.Token));

                // Small delay to ensure socket is ready
                await Task.Delay(100);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _listenerSocket != null)
            {
                try
                {
                    var clientSocket = await _listenerSocket.AcceptAsync(cancellationToken);
                    Console.WriteLine("FSKit IPC: Client connected");

                    // Handle each client in a separate task
                    _ = Task.Run(() => HandleClientAsync(clientSocket, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("FSKit IPC: Accept loop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        Console.WriteLine($"FSKit IPC: Error accepting connection: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Read message length prefix (4 bytes)
                    var lengthBuffer = new byte[4];
                    var bytesRead = await clientSocket.ReceiveAsync(lengthBuffer, SocketFlags.None, cancellationToken);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("FSKit IPC: Client disconnected");
                        break;
                    }

                    if (bytesRead != 4)
                    {
                        Console.WriteLine("FSKit IPC: Invalid message length prefix");
                        break;
                    }

                    var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // Validate message length
                    if (messageLength <= 0 || messageLength > 10 * 1024 * 1024) // 10MB max
                    {
                        Console.WriteLine($"FSKit IPC: Invalid message length: {messageLength}");
                        break;
                    }

                    // Read message
                    var messageBuffer = new byte[messageLength];
                    var totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        bytesRead = await clientSocket.ReceiveAsync(
                            messageBuffer.AsMemory(totalRead),
                            SocketFlags.None,
                            cancellationToken);

                        if (bytesRead == 0)
                            break;

                        totalRead += bytesRead;
                    }

                    if (totalRead != messageLength)
                    {
                        Console.WriteLine("FSKit IPC: Incomplete message received");
                        break;
                    }

                    var messageJson = Encoding.UTF8.GetString(messageBuffer);
                    Console.WriteLine($"FSKit IPC: Received message: {messageJson}");

                    // Deserialize request
                    IpcRequest? request;
                    try
                    {
                        request = JsonSerializer.Deserialize<IpcRequest>(messageJson);
                        if (request == null)
                        {
                            Console.WriteLine("FSKit IPC: Failed to deserialize request");
                            break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"FSKit IPC: JSON deserialization error: {ex.Message}");
                        break;
                    }

                    // Process request
                    var response = await _requestHandler.HandleRequestAsync(request, cancellationToken);

                    // Serialize and send response
                    var responseJson = JsonSerializer.Serialize(response);
                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    var responseLengthPrefix = BitConverter.GetBytes(responseBytes.Length);

                    await clientSocket.SendAsync(responseLengthPrefix, SocketFlags.None, cancellationToken);
                    await clientSocket.SendAsync(responseBytes, SocketFlags.None, cancellationToken);

                    Console.WriteLine($"FSKit IPC: Sent response: {responseJson}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("FSKit IPC: Client handler cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSKit IPC: Error handling client: {ex.Message}");
            }
            finally
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // Ignore shutdown errors
                }

                clientSocket.Dispose();
            }
        }

        public async Task StopAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                if (!_isRunning)
                {
                    Console.WriteLine("FSKit IPC: Server is not running");
                    return;
                }

                _isRunning = false;
                _cancellationTokenSource?.Cancel();

                try
                {
                    _listenerSocket?.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing listener socket: {ex.Message}");
                }

                // Clean up socket file
                if (File.Exists(_socketPath))
                {
                    try
                    {
                        File.Delete(_socketPath);
                        Console.WriteLine($"FSKit IPC: Deleted socket file at {_socketPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not delete socket file: {ex.Message}");
                    }
                }

                Console.WriteLine("FSKit IPC server stopped");
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            _listenerSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _requestHandler.Dispose();
            _startStopLock.Dispose();
        }
    }
}