using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FSKit;

/// <summary>
/// Unix domain socket IPC server for FSKit service.
/// Listens for connections and processes messages from the Uno app.
/// </summary>
public class FSKitIPCServer : IDisposable
{
    private readonly string _socketPath;
    private Socket? _listenerSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;

    public FSKitIPCServer()
    {
        // Use a short path in the user's home directory to stay under the 104 character limit
        // Unix domain socket paths have a maximum length of 104 characters on macOS
        // We need to get the REAL home directory, not the sandboxed one

        // Get the actual home directory by expanding ~
        var homePath = Environment.GetEnvironmentVariable("HOME")
                      ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var socketDir = Path.Combine(homePath, ".securefolder");

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
        Console.WriteLine($"FSKit IPC: Socket path configured as {_socketPath} (length: {_socketPath.Length})");
    }

    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        // Clean up any existing socket file
        if (File.Exists(_socketPath))
        {
            try
            {
                File.Delete(_socketPath);
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
                break;
            }
            catch (Exception ex)
            {
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

                var message = Encoding.UTF8.GetString(messageBuffer);
                Console.WriteLine($"FSKit IPC: Received message: '{message}'");

                // Process message and send response
                var response = ProcessMessage(message);
                var responseBytes = Encoding.UTF8.GetBytes(response);
                var responseLengthPrefix = BitConverter.GetBytes(responseBytes.Length);

                await clientSocket.SendAsync(responseLengthPrefix, SocketFlags.None, cancellationToken);
                await clientSocket.SendAsync(responseBytes, SocketFlags.None, cancellationToken);

                Console.WriteLine($"FSKit IPC: Sent response: '{response}'");
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
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

    private string ProcessMessage(string message)
    {
        // Simple echo service for demonstration
        // Replace this with actual FSKit functionality
        return message switch
        {
            "ping" => "pong",
            "hello" => "Hello from FSKit service!",
            _ => $"Echo: {message}"
        };
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete socket file: {ex.Message}");
            }
        }

        Console.WriteLine("FSKit IPC server stopped");
    }

    public void Dispose()
    {
        Stop();
        _listenerSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

