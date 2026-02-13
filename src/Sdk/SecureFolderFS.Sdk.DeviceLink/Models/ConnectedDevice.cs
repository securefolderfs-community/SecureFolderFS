using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DeviceLink.Enums;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.DeviceLink.Models
{
    public sealed class ConnectedDevice : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private bool _disposed;

        public Stream DeviceStream { get; }

        /// <summary>
        /// Gets a value indicating whether the device is connected.
        /// </summary>
        public bool IsConnected => !_disposed && _tcpClient.Connected;

        private ConnectedDevice(TcpClient tcpClient, Stream deviceStream)
        {
            _tcpClient = tcpClient;
            DeviceStream = deviceStream;
        }

        public async Task SendMessageAsync(byte[] message, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var lengthBytes = BitConverter.GetBytes(message.Length);
            await DeviceStream.WriteAsync(lengthBytes, cancellationToken);
            await DeviceStream.WriteAsync(message, cancellationToken);
            await DeviceStream.FlushAsync(cancellationToken);
        }

        public async Task SendMessageAsync(byte[] payload, MessageType type, CancellationToken cancellationToken)
        {
            var message = new byte[1 + payload.Length];
            message[0] = (byte)type;
            payload.CopyTo(message, 1);
            await SendMessageAsync(message, cancellationToken);
        }

        public async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var lengthBytes = new byte[4];
            var bytesRead = await DeviceStream.ReadAsync(lengthBytes, cancellationToken);
            if (bytesRead == 0)
                throw new IOException("Connection closed by remote host.");
            if (bytesRead < 4)
                throw new IOException("Connection closed unexpectedly.");

            var length = BitConverter.ToInt32(lengthBytes);
            if (length <= 0 || length > 1024 * 1024) // Max 1MB message
                throw new IOException($"Invalid message length: {length}");

            var message = new byte[length];
            var totalRead = 0;

            while (totalRead < length)
            {
                bytesRead = await DeviceStream.ReadAsync(message.AsMemory(totalRead, length - totalRead), cancellationToken);
                if (bytesRead == 0)
                    throw new IOException("Connection closed unexpectedly.");

                totalRead += bytesRead;
            }

            return message;
        }

        public static async Task<ConnectedDevice> ConnectAsync(DiscoveredDevice discoveredDevice, CancellationToken cancellationToken)
        {
            var tcpClient = new TcpClient();
            try
            {
                // Configure for low latency before connecting
                tcpClient.NoDelay = true;

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(Constants.CONNECTION_TIMEOUT_MS);

                await tcpClient.ConnectAsync(discoveredDevice.IpAddress, discoveredDevice.Port, timeoutCts.Token);
                var stream = tcpClient.GetStream();

                return new ConnectedDevice(tcpClient, stream);
            }
            catch
            {
                tcpClient.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a <see cref="ConnectedDevice"/> from an already accepted <see cref="TcpClient"/>.
        /// </summary>
        /// <param name="tcpClient">The accepted TCP client.</param>
        /// <returns>A new <see cref="ConnectedDevice"/> instance.</returns>
        public static ConnectedDevice FromTcpClient(TcpClient tcpClient)
        {
            return new ConnectedDevice(tcpClient, tcpClient.GetStream());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            SafetyHelpers.NoFailure(() => DeviceStream.Dispose());
            SafetyHelpers.NoFailure(() => _tcpClient.Dispose());
        }
    }
}
