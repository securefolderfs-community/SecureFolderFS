using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.Services;

/// <summary>
/// Client that communicates with FSKit service via Unix domain socket.
/// Works on net10.0-desktop without AppKit/XPC dependencies.
/// </summary>
public class FSKitIPCClient : IDisposable
{
    private readonly string _socketPath;
    private Socket? _socket;
    private bool _isConnected;

    public FSKitIPCClient()
    {
        // Use a short path in the user's home directory to stay under the 104 character limit
        // Unix domain socket paths have a maximum length of 104 characters on macOS
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var socketDir = Path.Combine(homePath, ".securefolder");
        
        // Ensure directory exists
        if (!Directory.Exists(socketDir))
            Directory.CreateDirectory(socketDir);
        
        _socketPath = Path.Combine(socketDir, "fskit.sock");
        Debug.WriteLine($"FSKit IPC Client: Socket path is {_socketPath} (length: {_socketPath.Length})");
    }

    public async Task<bool> ConnectAsync(int retries = 5, int delayMs = 500)
    {
        if (_isConnected)
            return true;

        for (int i = 0; i < retries; i++)
        {
            try
            {
                if (!File.Exists(_socketPath))
                {
                    Debug.WriteLine($"FSKit IPC: Socket not found at {_socketPath}, retrying... ({i + 1}/{retries})");
                    await Task.Delay(delayMs);
                    continue;
                }

                _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                var endpoint = new UnixDomainSocketEndPoint(_socketPath);
                
                await _socket.ConnectAsync(endpoint);
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
                    await Task.Delay(delayMs);
            }
        }

        return false;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (!_isConnected || _socket == null)
            throw new InvalidOperationException("Not connected to FSKit service. Call ConnectAsync first.");

        try
        {
            // Send message length prefix (4 bytes) + message
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);
            
            await _socket.SendAsync(lengthPrefix, SocketFlags.None);
            await _socket.SendAsync(messageBytes, SocketFlags.None);

            // Read response length prefix
            var responseLengthBuffer = new byte[4];
            var bytesRead = await _socket.ReceiveAsync(responseLengthBuffer, SocketFlags.None);
            if (bytesRead != 4)
                throw new IOException("Failed to read response length");

            var responseLength = BitConverter.ToInt32(responseLengthBuffer, 0);
            
            // Read response message
            var responseBuffer = new byte[responseLength];
            var totalRead = 0;
            while (totalRead < responseLength)
            {
                bytesRead = await _socket.ReceiveAsync(responseBuffer.AsMemory(totalRead), SocketFlags.None);
                if (bytesRead == 0)
                    throw new IOException("Connection closed while reading response");
                totalRead += bytesRead;
            }

            var response = Encoding.UTF8.GetString(responseBuffer);
            Debug.WriteLine($"FSKit IPC: Sent '{message}', received '{response}'");
            return response;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FSKit IPC: Communication error: {ex.Message}");
            _isConnected = false;
            throw;
        }
    }

    public void Dispose()
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
}
