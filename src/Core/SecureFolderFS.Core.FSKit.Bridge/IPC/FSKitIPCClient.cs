using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Ipc;

namespace SecureFolderFS.Core.FSKit.Bridge.IPC
{
    /// <summary>
    /// IPC client for communicating with FSKit service via Unix domain socket.
    /// </summary>
    internal sealed class FSKitIPCClient : IDisposable
    {
        private readonly string _socketPath;
        private Socket? _socket;
        private bool _isConnected;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public FSKitIPCClient()
        {
            // Use a short path in the user's home directory to stay under the 104 character limit
            var homePath = Environment.GetEnvironmentVariable("HOME")
                          ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var socketDir = Path.Combine(homePath, Sdk.Ipc.Constants.Sockets.SOCKET_DIRECTORY_NAME);

            // Ensure directory exists
            if (!Directory.Exists(socketDir))
                Directory.CreateDirectory(socketDir);

            _socketPath = Path.Combine(socketDir, "fskit.sock");
            Debug.WriteLine($"FSKit IPC Client: Socket path is {_socketPath} (length: {_socketPath.Length})");
        }

        public bool IsConnected => _isConnected && _socket?.Connected == true;

        public async Task<bool> ConnectAsync(int retries = 10, int delayMs = 500, CancellationToken cancellationToken = default)
        {
            if (_isConnected && _socket?.Connected == true)
                return true;

            for (int i = 0; i < retries; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                try
                {
                    if (!File.Exists(_socketPath))
                    {
                        Debug.WriteLine($"FSKit IPC: Socket not found at {_socketPath}, retrying... ({i + 1}/{retries})");
                        await Task.Delay(delayMs, cancellationToken);
                        continue;
                    }

                    _socket?.Dispose();
                    _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var endpoint = new UnixDomainSocketEndPoint(_socketPath);

                    await _socket.ConnectAsync(endpoint, cancellationToken);
                    _isConnected = true;
                    Debug.WriteLine($"FSKit IPC: Connected to socket at {_socketPath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FSKit IPC: Connection attempt {i + 1} failed: {ex.Message}");
                    _socket?.Dispose();
                    _socket = null;

                    if (i < retries - 1)
                        await Task.Delay(delayMs, cancellationToken);
                }
            }

            return false;
        }

        public async Task<IpcResponse> SendRequestAsync(IpcRequest request, CancellationToken cancellationToken = default)
        {
            if (!_isConnected || _socket == null)
                throw new InvalidOperationException("Not connected to FSKit service. Call ConnectAsync first.");

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                // Serialize request to JSON
                var json = JsonSerializer.Serialize(request);
                var messageBytes = Encoding.UTF8.GetBytes(json);
                var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

                // Send length prefix + message
                var bytesSent1 = await _socket.SendAsync(lengthPrefix, SocketFlags.None, cancellationToken);
                var bytesSent2 = await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);

                Debug.WriteLine($"FSKit IPC: Sent {bytesSent1 + bytesSent2} bytes for request: {request.Command} (RequestId: {request.RequestId})");
                //Debug.WriteLine($"FSKit IPC: Sent request: {request.Command} (RequestId: {request.RequestId})");

                // Read response length prefix
                var responseLengthBuffer = new byte[4];
                var bytesRead = await _socket.ReceiveAsync(responseLengthBuffer, SocketFlags.None, cancellationToken);
                if (bytesRead != 4)
                    throw new IOException("Failed to read response length");

                var responseLength = BitConverter.ToInt32(responseLengthBuffer, 0);

                // Validate response length
                if (responseLength <= 0 || responseLength > 10 * 1024 * 1024) // 10MB max
                    throw new IOException($"Invalid response length: {responseLength}");

                // Read response message
                var responseBuffer = new byte[responseLength];
                var totalRead = 0;
                while (totalRead < responseLength)
                {
                    bytesRead = await _socket.ReceiveAsync(
                        responseBuffer.AsMemory(totalRead),
                        SocketFlags.None,
                        cancellationToken);

                    if (bytesRead == 0)
                        throw new IOException("Connection closed while reading response");

                    totalRead += bytesRead;
                }

                // Deserialize response
                var responseJson = Encoding.UTF8.GetString(responseBuffer);
                var response = JsonSerializer.Deserialize<IpcResponse>(responseJson)
                    ?? throw new InvalidOperationException("Failed to deserialize response");

                Debug.WriteLine($"FSKit IPC: Received response: {response.Status} (RequestId: {response.RequestId})");
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FSKit IPC: Communication error: {ex.Message}");
                _isConnected = false;
                throw;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Disconnect()
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // Ignore shutdown errors
                }

                _socket.Dispose();
                _socket = null;
            }

            _isConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
            _sendLock.Dispose();
        }
    }
}

