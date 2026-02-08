using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Helpers;
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
        private readonly object _lock = new();
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
            lock (_lock)
            {
                if (IsListening || _disposed)
                    return Task.CompletedTask;

                _listenerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // Start UDP discovery responder with socket reuse
                _udpClient = new UdpClient();
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));
                _ = Task.Run(() => ListenForDiscoveryAsync(_listenerCts.Token));

                // Start TCP listener with socket reuse and NoDelay for faster connections
                _tcpListener = new TcpListener(IPAddress.Any, COMMUNICATION_PORT);
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _tcpListener.Start();
                _ = Task.Run(() => AcceptConnectionsAsync(_listenerCts.Token));

                IsListening = true;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops listening for discovery requests and TCP connections.
        /// </summary>
        public void StopListening()
        {
            lock (_lock)
            {
                if (!IsListening)
                    return;

                IsListening = false;

                SafetyHelpers.NoFailure(() => _listenerCts?.Cancel());
                SafetyHelpers.NoFailure(() => _udpClient?.Close());
                SafetyHelpers.NoFailure(() => _tcpListener?.Stop());

                CloseCurrentConnection();
            }
        }

        /// <summary>
        /// Closes the current connection if any.
        /// </summary>
        public void CloseCurrentConnection()
        {
            var connection = Interlocked.Exchange(ref _currentConnection, null);
            SafetyHelpers.NoFailure(() => connection?.Dispose());
        }

        private async Task ListenForDiscoveryAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !_disposed)
            {
                try
                {
                    var result = await _udpClient!.ReceiveAsync(cancellationToken);
                    if (!ProtocolSerializer.ValidateDiscoveryRequest(result.Buffer))
                        continue;

                    var response = ProtocolSerializer.CreateDiscoveryResponse(_deviceId, _deviceName, COMMUNICATION_PORT);
                    await _udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    // Socket error - wait a bit and continue if not cancelled
                    if (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(100, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
                catch (Exception)
                {
                    // Log or handle other discovery errors if needed
                    if (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(100, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
            }
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !_disposed)
            {
                try
                {
                    var tcpClient = await _tcpListener!.AcceptTcpClientAsync(cancellationToken);

                    // Configure for low latency
                    tcpClient.NoDelay = true;

                    var connectedDevice = ConnectedDevice.FromTcpClient(tcpClient);

                    // Store and close previous connection if any
                    var previousConnection = Interlocked.Exchange(ref _currentConnection, connectedDevice);
                    SafetyHelpers.NoFailure(() => previousConnection?.Dispose());

                    // Notify subscribers about the new connection
                    ConnectionAccepted?.Invoke(this, connectedDevice);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    // Socket error - wait a bit and continue if not cancelled
                    if (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(100, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
                catch (Exception)
                {
                    // Log or handle other connection errors if needed
                    if (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(100, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopListening();

            SafetyHelpers.NoFailure(() => _listenerCts?.Dispose());
            SafetyHelpers.NoFailure(() => _udpClient?.Dispose());

            ConnectionAccepted = null;
        }
    }
}