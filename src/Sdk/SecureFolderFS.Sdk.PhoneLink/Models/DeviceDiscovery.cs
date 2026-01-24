// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Sockets;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace SecureFolderFS.Sdk.PhoneLink.Models;
//
// public class DeviceDiscovery : IDisposable
// {
//     private readonly string _desktopName;
//     private CancellationTokenSource? _discoveryCts;
//     private bool _disposed;
//
//     /// <summary>
//     /// Event raised when a new device is discovered.
//     /// </summary>
//     public event EventHandler<DiscoveredDevice>? DeviceDiscovered;
//
//     public DeviceDiscovery(string desktopName = "SecureFolderFS Desktop")
//     {
//         _desktopName = desktopName;
//     }
//
//     /// <summary>
//     /// Starts discovering devices on the local network.
//     /// </summary>
//     public async Task<List<DiscoveredDevice>> DiscoverDevicesAsync(
//         int timeoutMs = Constants.DISCOVERY_TIMEOUT_MS,
//         CancellationToken cancellationToken = default)
//     {
//         var devices = new List<DiscoveredDevice>();
//         _discoveryCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
//
//         // Create a new UdpClient for each discovery operation
//         using var udpClient = new UdpClient();
//         udpClient.EnableBroadcast = true;
//
//         try
//         {
//             // Bind to receive responses
//             udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
//
//             // Send broadcast discovery request
//             var discoveryPacket = ProtocolSerializer.CreateDiscoveryRequest(_desktopName);
//             var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, Constants.DISCOVERY_PORT);
//
//             await udpClient.SendAsync(discoveryPacket, discoveryPacket.Length, broadcastEndpoint);
//
//             // Listen for responses
//             var timeoutTask = Task.Delay(timeoutMs, _discoveryCts.Token);
//
//             while (!_discoveryCts.Token.IsCancellationRequested)
//             {
//                 var receiveTask = udpClient.ReceiveAsync(_discoveryCts.Token);
//                 var completedTask = await Task.WhenAny(receiveTask.AsTask(), timeoutTask);
//
//                 if (completedTask == timeoutTask)
//                     break;
//
//                 var result = await receiveTask;
//                 var device = ProtocolSerializer.ParseDiscoveryResponse(
//                     result.Buffer,
//                     result.RemoteEndPoint.Address.ToString());
//
//                 if (device != null)
//                 {
//                     devices.Add(device);
//                     DeviceDiscovered?.Invoke(this, device);
//                 }
//             }
//         }
//         catch (OperationCanceledException)
//         {
//         }
//         catch (Exception ex)
//         {
//             _ = ex;
//         }
//
//         return devices;
//     }
//
//     /// <summary>
//     /// Stops the current discovery operation.
//     /// </summary>
//     public void StopDiscovery()
//     {
//         _discoveryCts?.Cancel();
//     }
//
//     public void Dispose()
//     {
//         if (_disposed) return;
//         _disposed = true;
//
//         _discoveryCts?.Cancel();
//         _discoveryCts?.Dispose();
//     }
// }