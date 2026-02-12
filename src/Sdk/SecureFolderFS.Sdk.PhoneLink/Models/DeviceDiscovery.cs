using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    /// <summary>
    /// Discovers mobile devices on the local network via UDP broadcast.
    /// </summary>
    public sealed class DeviceDiscovery : IDisposable
    {
        private readonly string _desktopName;
        private CancellationTokenSource? _discoveryCts;
        private bool _disposed;

        /// <summary>
        /// Event raised when a new device is discovered.
        /// </summary>
        public event EventHandler<DiscoveredDevice>? DeviceDiscovered;

        public DeviceDiscovery(string desktopName)
        {
            _desktopName = desktopName;
        }

        /// <summary>
        /// Starts discovering devices on the local network.
        /// </summary>
        /// <param name="timeoutMs">Discovery timeout in milliseconds.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A list of discovered devices.</returns>
        public async Task<List<DiscoveredDevice>> DiscoverDevicesAsync(
            int timeoutMs = Constants.DISCOVERY_TIMEOUT_MS,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var devices = new List<DiscoveredDevice>();
            _discoveryCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            UdpClient? udpClient = null;
            try
            {
                // Create a new UdpClient for each discovery operation
                udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Set receive timeout for faster response
                udpClient.Client.ReceiveTimeout = timeoutMs;

                // Bind to receive responses
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

                // Send broadcast discovery request
                var discoveryPacket = ProtocolSerializer.CreateDiscoveryRequest(_desktopName);
                var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, Constants.DISCOVERY_PORT);

                // Send multiple times for reliability (UDP can be lost)
                await udpClient.SendAsync(discoveryPacket, discoveryPacket.Length, broadcastEndpoint);

                // Small delay then send again for reliability
                await Task.Delay(50, cancellationToken);
                await udpClient.SendAsync(discoveryPacket, discoveryPacket.Length, broadcastEndpoint);

                // Listen for responses with timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_discoveryCts.Token);
                timeoutCts.CancelAfter(timeoutMs);

                while (!timeoutCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync(timeoutCts.Token);
                        var device = ProtocolSerializer.ParseDiscoveryResponse(
                            result.Buffer,
                            result.RemoteEndPoint.Address.ToString());

                        if (device != null && !devices.Exists(d => d.DeviceId == device.DeviceId))
                        {
                            devices.Add(device);
                            DeviceDiscovered?.Invoke(this, device);

                            // If we found a device, we can return early for faster response
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (SocketException)
                    {
                        // Socket error during receive - continue if not cancelled
                        if (timeoutCts.Token.IsCancellationRequested)
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Discovery was cancelled or timed out - this is normal
            }
            catch (SocketException)
            {
                // Socket error - return whatever devices we found
            }
            finally
            {
                SafetyHelpers.NoFailure(() => udpClient?.Close());
                SafetyHelpers.NoFailure(() => udpClient?.Dispose());
            }

            return devices;
        }

        /// <summary>
        /// Stops the current discovery operation.
        /// </summary>
        public void StopDiscovery()
        {
            SafetyHelpers.NoFailure(() => _discoveryCts?.Cancel());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopDiscovery();
            SafetyHelpers.NoFailure(() => _discoveryCts?.Dispose());

            DeviceDiscovered = null;
        }
    }
}