using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    /// <summary>
    /// Handles both UDP discovery responses and TCP connection acceptance for the mobile device.
    /// This is the mobile-side network listener counterpart to <see cref="DeviceDiscovery"/>.
    /// </summary>
    public sealed class DeviceConnectionListener : IDisposable
    {
        private readonly string _deviceId;
        private readonly string _deviceName;
        private UdpClient? _udpClient;
        private TcpListener? _tcpListener;
        private ConnectedDevice? _currentConnection;
        private CancellationTokenSource? _listenerCts;
        private bool _disposed;

        /// <summary>
        /// Event raised when a new connection is accepted.
        /// </summary>
        public event EventHandler<ConnectedDevice>? ConnectionAccepted;

        /// <summary>
        /// Gets a value indicating whether the listener is currently active.
        /// </summary>
        public bool IsListening { get; private set; }

        public DeviceConnectionListener(string deviceId, string deviceName)
        {
            _deviceId = deviceId;
            _deviceName = deviceName;
        }

        /// <summary>
        /// Starts listening for UDP discovery requests and TCP connections.
        /// </summary>
        public Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            if (IsListening)
                return Task.CompletedTask;

            _listenerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start UDP discovery responder
            _udpClient = new UdpClient(DISCOVERY_PORT);
            _ = Task.Run(() => ListenForDiscoveryAsync(_listenerCts.Token));

            // Start TCP listener
            _tcpListener = new TcpListener(IPAddress.Any, COMMUNICATION_PORT);
            _tcpListener.Start();
            _ = Task.Run(() => AcceptConnectionsAsync(_listenerCts.Token));

            IsListening = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops listening for discovery requests and TCP connections.
        /// </summary>
        public void StopListening()
        {
            _listenerCts?.Cancel();
            _udpClient?.Close();
            _tcpListener?.Stop();
            _currentConnection?.Dispose();

            IsListening = false;
        }

        /// <summary>
        /// Closes the current connection if any.
        /// </summary>
        public void CloseCurrentConnection()
        {
            var connection = Interlocked.Exchange(ref _currentConnection, null);
            try
            {
                connection?.Dispose();
            }
            catch
            {
                /* Ignore disposal errors */
            }
        }

        private async Task ListenForDiscoveryAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _udpClient!.ReceiveAsync(cancellationToken);
                    if (!ProtocolSerializer.ValidateDiscoveryRequest(result.Buffer))
                        continue;

                    var response =
                        ProtocolSerializer.CreateDiscoveryResponse(_deviceId, _deviceName, COMMUNICATION_PORT);
                    await _udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                // Log or handle discovery errors if needed
            }
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener!.AcceptTcpClientAsync(cancellationToken);
                    var connectedDevice = ConnectedDevice.FromTcpClient(tcpClient);

                    // Store and close previous connection if any
                    var previousConnection = Interlocked.Exchange(ref _currentConnection, connectedDevice);
                    try
                    {
                        previousConnection?.Dispose();
                    }
                    catch
                    {
                        /* Ignore errors closing old connection */
                    }

                    // Notify subscribers about the new connection
                    ConnectionAccepted?.Invoke(this, connectedDevice);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                // Log or handle connection errors if needed
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopListening();
            _listenerCts?.Dispose();
            _udpClient?.Dispose();
            _tcpListener?.Stop();

            ConnectionAccepted = null;
        }
    }
}