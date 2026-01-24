// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Net.Sockets;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using SecureFolderFS.Sdk.PhoneLink.Enums;
//
// namespace SecureFolderFS.Sdk.PhoneLink.Models
// {
//     public sealed class SecurePhoneLinkAuthenticator : IDisposable
//     {
//         private readonly DeviceDiscovery _discovery;
//         private readonly VaultConfigurationStore _configStore;
//         private readonly string _desktopName;
//
//         private TcpClient? _tcpClient;
//         private NetworkStream? _stream;
//         private SecureChannelModel? _secureChannel;
//         private ECDiffieHellman? _ecdhKeyPair;
//         private byte[]? _sharedSecret;
//         private DiscoveredDevice? _connectedDevice;
//         private bool _disposed;
//
//         /// <summary>
//         /// Challenge validity window in seconds.
//         /// Challenges older than this are rejected.
//         /// </summary>
//         public const int ChallengeValiditySeconds = 30;
//
//         /// <summary>
//         /// Event raised when a device is discovered.
//         /// </summary>
//         public event EventHandler<DiscoveredDevice>? DeviceDiscovered;
//
//         /// <summary>
//         /// Event raised when verification code is available during pairing.
//         /// Display this code to the user for confirmation.
//         /// </summary>
//         public event EventHandler<string>? VerificationCodeReady;
//
//         public SecurePhoneLinkAuthenticator(string desktopName = "SecureFolderFS Desktop",
//             string? configBasePath = null)
//         {
//             _desktopName = desktopName;
//             _discovery = new DeviceDiscovery(desktopName);
//             _configStore = new VaultConfigurationStore(configBasePath);
//
//             _discovery.DeviceDiscovered += (_, device) => DeviceDiscovered?.Invoke(this, device);
//         }
//
//         #region Device Discovery
//
//         /// <summary>
//         /// Discovers available mobile devices on the local network.
//         /// </summary>
//         public Task<List<DiscoveredDevice>> DiscoverDevicesAsync(
//             int timeoutMs = Constants.DISCOVERY_TIMEOUT_MS,
//             CancellationToken cancellationToken = default)
//         {
//             return _discovery.DiscoverDevicesAsync(timeoutMs, cancellationToken);
//         }
//
//         /// <summary>
//         /// Waits until a device is discovered.
//         /// </summary>
//         public async Task<DiscoveredDevice?> WaitForDeviceAsync(
//             int scanIntervalMs = 2000,
//             int singleScanTimeoutMs = 3000,
//             CancellationToken cancellationToken = default)
//         {
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 var devices = await DiscoverDevicesAsync(singleScanTimeoutMs, cancellationToken);
//
//                 if (devices.Count > 0)
//                     return devices[0];
//
//                 await Task.Delay(scanIntervalMs, cancellationToken);
//             }
//
//             return null;
//         }
//
//         #endregion
//
//         #region Secure Pairing (Enrollment)
//
//         /// <summary>
//         /// Initiates secure pairing with a mobile device.
//         /// This creates a new credential binding for the vault.
//         /// </summary>
//         /// <param name="device">The discovered device to pair with.</param>
//         /// <param name="vaultId">The vault's unique identifier.</param>
//         /// <param name="vaultName">Human-readable vault name.</param>
//         /// <param name="confirmVerificationCode">Callback to confirm the verification code matches.</param>
//         /// <param name="cancellationToken">Cancellation token.</param>
//         /// <returns>The pairing configuration if successful, null otherwise.</returns>
//         public async Task<VaultPhoneLinkConfig?> PairWithDeviceAsync(
//             DiscoveredDevice device,
//             string vaultId,
//             string vaultName,
//             Func<string, Task<bool>> confirmVerificationCode,
//             CancellationToken cancellationToken = default)
//         {
//             try
//             {
//                 // Step 1: Connect to device
//                 await ConnectToDeviceAsync(device, cancellationToken);
//
//                 // Step 2: Generate ECDH keypair for key exchange
//                 _ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
//                 var myPublicKey = _ecdhKeyPair.ExportSubjectPublicKeyInfo();
//
//                 // Step 3: Send pairing request with our ECDH public key
//                 var pairingRequest = CreatePairingRequest(myPublicKey);
//                 await SendMessageAsync(pairingRequest, cancellationToken);
//
//                 // Step 4: Receive pairing response with mobile's ECDH public key
//                 var response = await ReceiveMessageAsync(cancellationToken);
//                 var messageType = (MessageType)response[0];
//
//                 if (messageType == MessageType.PairingRejected)
//                     return null;
//
//                 if (messageType != MessageType.PairingResponse)
//                     return null;
//
//                 var mobileEcdhPublicKey = ParsePairingResponse(response);
//
//                 // Step 5: Derive shared secret using ECDH
//                 _sharedSecret = SecureChannelModel.DeriveSharedSecret(_ecdhKeyPair, mobileEcdhPublicKey);
//
//                 // Step 6: Compute and display verification code
//                 var verificationCode = SecureChannelModel.ComputeVerificationCode(_sharedSecret);
//                 VerificationCodeReady?.Invoke(this, verificationCode);
//
//                 // Step 7: User confirms the code matches
//                 var codeConfirmed = await confirmVerificationCode(verificationCode);
//
//                 if (!codeConfirmed)
//                 {
//                     await SendPairingRejectedAsync(cancellationToken);
//                     return null;
//                 }
//
//                 // Step 8: Generate CID and send pairing confirmation
//                 var credentialId = Guid.NewGuid().ToString();
//                 var pairingId = Guid.NewGuid().ToString();
//
//                 var confirmMessage = CreatePairingConfirmMessage(credentialId, vaultName, pairingId);
//                 await SendMessageAsync(confirmMessage, cancellationToken);
//
//                 // Step 9: Receive pairing complete with signing public key
//                 var completeResponse = await ReceiveMessageAsync(cancellationToken);
//                 messageType = (MessageType)completeResponse[0];
//
//                 if (messageType != MessageType.PairingComplete)
//                     return null;
//
//                 var signingPublicKey = ParsePairingComplete(completeResponse);
//
//                 // Step 10: Create and save configuration
//                 var config = new VaultPhoneLinkConfig
//                 {
//                     CredentialId = credentialId,
//                     VaultName = vaultName,
//                     MobileDeviceId = device.DeviceId,
//                     MobileDeviceName = device.DeviceName,
//                     PublicSigningKey = Convert.ToBase64String(signingPublicKey),
//                     PairingId = pairingId,
//                     CreatedAt = DateTime.UtcNow
//                 };
//
//                 await _configStore.SaveConfigurationAsync(vaultId, config, _sharedSecret);
//                 return config;
//             }
//             catch (Exception ex)
//             {
//                 return null;
//             }
//             finally
//             {
//                 Disconnect();
//             }
//         }
//
//         #endregion
//
//         #region Secure Authentication
//
//         /// <summary>
//         /// Authenticates with a paired mobile device to unlock a vault.
//         /// Uses the stored pairing configuration and shared secret.
//         /// </summary>
//         /// <param name="vaultId">The vault's unique identifier.</param>
//         /// <param name="cancellationToken">Cancellation token.</param>
//         /// <returns>The derived vault key, or null if authentication failed.</returns>
//         public async Task<byte[]?> AuthenticateAsync(
//             string vaultId,
//             CancellationToken cancellationToken = default)
//         {
//             try
//             {
//                 // Step 1: Load vault configuration
//                 var config = await _configStore.LoadConfigurationAsync(vaultId);
//                 if (config == null)
//                     return null;
//
//                 // Step 2: Load shared secret
//                 _sharedSecret = await _configStore.GetSharedSecretAsync(vaultId, config.CredentialId);
//                 if (_sharedSecret == null)
//                     return null;
//
//                 // Step 3: Discover and connect to the paired device
//                 var device = await DiscoverPairedDeviceAsync(config, cancellationToken);
//                 if (device == null)
//                     return null;
//
//                 await ConnectToDeviceAsync(device, cancellationToken);
//
//                 // Step 4: Establish secure session
//                 var sessionEstablished = await EstablishSecureSessionAsync(config, cancellationToken);
//                 if (!sessionEstablished)
//                     return null;
//
//                 // Step 5: Send secure authentication request
//                 var challenge = new byte[Constants.ChallengeSize];
//                 RandomNumberGenerator.Fill(challenge);
//
//                 var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//                 var nonce = new byte[16];
//                 RandomNumberGenerator.Fill(nonce);
//
//                 var authRequest = CreateSecureAuthRequest(config.CredentialId, challenge, timestamp, nonce);
//                 var encryptedRequest = _secureChannel!.Encrypt(authRequest);
//                 await SendMessageAsync(encryptedRequest, MessageType.SecureAuthRequest, cancellationToken);
//
//                 // Step 6: Receive encrypted response
//                 var encryptedResponse = await ReceiveMessageAsync(cancellationToken);
//                 var messageType = (MessageType)encryptedResponse[0];
//
//                 if (messageType == MessageType.AuthenticationRejected)
//                 {
//                     var reason = ParseAuthenticationRejected(encryptedResponse);
//                     return null;
//                 }
//
//                 if (messageType != MessageType.SecureAuthResponse)
//                     return null;
//
//                 // Decrypt response
//                 var responsePayload = encryptedResponse.AsSpan(1).ToArray();
//                 var decryptedResponse = _secureChannel.Decrypt(responsePayload);
//                 var signature = decryptedResponse;
//
//                 // Step 7: Verify signature
//                 using var verifyKey = ECDsa.Create();
//                 verifyKey.ImportSubjectPublicKeyInfo(config.PublicSigningKeyBytes, out _);
//
//                 // The mobile signs: CID + challenge + timestamp + nonce
//                 var signedData = BuildSignedData(config.CredentialId, challenge, timestamp, nonce);
//                 var isValid = verifyKey.VerifyData(signedData, signature, HashAlgorithmName.SHA256);
//
//                 if (!isValid)
//                     return null;
//
//                 // Step 8: Derive vault key using HKDF from the SHARED SECRET
//                 // The shared secret is deterministic (established during pairing)
//                 // The signature verification proves the mobile has the correct signing key
//                 // We use CID as context to bind the key to this specific vault
//                 var info = Encoding.UTF8.GetBytes($"PhoneLink-VaultKey-{config.CredentialId}");
//                 var salt = Encoding.UTF8.GetBytes(config.PairingId);
//                 var derivedKey = HKDF.DeriveKey(
//                     HashAlgorithmName.SHA256,
//                     _sharedSecret!, // Use shared secret, not signature
//                     32,
//                     salt,
//                     info);
//
//                 return derivedKey;
//             }
//             catch (Exception ex)
//             {
//                 return null;
//             }
//             finally
//             {
//                 Disconnect();
//             }
//         }
//
//         /// <summary>
//         /// Waits for a device and authenticates - all in one call.
//         /// </summary>
//         public async Task<byte[]?> WaitAndAuthenticateAsync(
//             string vaultId,
//             CancellationToken cancellationToken = default)
//         {
//             var config = await _configStore.LoadConfigurationAsync(vaultId);
//             if (config == null)
//                 return null;
//
//             // Wait for the specific paired device
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 var devices = await DiscoverDevicesAsync(3000, cancellationToken);
//                 var pairedDevice = devices.FirstOrDefault(d => d.DeviceId == config.MobileDeviceId);
//
//                 if (pairedDevice != null)
//                     return await AuthenticateAsync(vaultId, cancellationToken);
//
//                 await Task.Delay(2000, cancellationToken);
//             }
//
//             return null;
//         }
//
//         #endregion
//
//         #region Private Helpers
//
//         private async Task ConnectToDeviceAsync(DiscoveredDevice device, CancellationToken cancellationToken)
//         {
//             _tcpClient = new TcpClient();
//
//             using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
//             timeoutCts.CancelAfter(Constants.DISCOVERY_TIMEOUT_MS);
//
//             await _tcpClient.ConnectAsync(device.IpAddress, device.Port, timeoutCts.Token);
//             _stream = _tcpClient.GetStream();
//             _connectedDevice = device;
//         }
//
//         private async Task<DiscoveredDevice?> DiscoverPairedDeviceAsync(
//             VaultPhoneLinkConfig config,
//             CancellationToken cancellationToken)
//         {
//             var devices = await DiscoverDevicesAsync(3000, cancellationToken);
//             return devices.FirstOrDefault(d => d.DeviceId == config.MobileDeviceId);
//         }
//
//         private async Task<bool> EstablishSecureSessionAsync(
//             VaultPhoneLinkConfig config,
//             CancellationToken cancellationToken)
//         {
//             // Generate session nonce
//             var sessionNonce = new byte[16];
//             RandomNumberGenerator.Fill(sessionNonce);
//
//             // Send secure session request
//             var sessionRequest = CreateSecureSessionRequest(config.PairingId, sessionNonce);
//             await SendMessageAsync(sessionRequest, cancellationToken);
//
//             // Receive response
//             var response = await ReceiveMessageAsync(cancellationToken);
//             var messageType = (MessageType)response[0];
//
//             if (messageType != MessageType.SecureSessionAccepted)
//                 return false;
//
//             var mobileNonce = ParseSecureSessionAccepted(response);
//
//             // Derive session key from shared secret + both nonces
//             var combinedNonce = new byte[sessionNonce.Length + mobileNonce.Length];
//             sessionNonce.CopyTo(combinedNonce, 0);
//             mobileNonce.CopyTo(combinedNonce, sessionNonce.Length);
//
//             _secureChannel = new SecureChannelModel(_sharedSecret!, combinedNonce);
//
//             return true;
//         }
//
//         private async Task SendPairingRejectedAsync(CancellationToken cancellationToken)
//         {
//             var message = new byte[] { (byte)MessageType.PairingRejected };
//             await SendMessageAsync(message, cancellationToken);
//         }
//
//         private async Task SendMessageAsync(byte[] message, CancellationToken cancellationToken)
//         {
//             if (_stream == null)
//                 throw new InvalidOperationException("Not connected");
//
//             var lengthBytes = BitConverter.GetBytes(message.Length);
//             await _stream.WriteAsync(lengthBytes, cancellationToken);
//             await _stream.WriteAsync(message, cancellationToken);
//             await _stream.FlushAsync(cancellationToken);
//         }
//
//         private async Task SendMessageAsync(byte[] payload, MessageType type, CancellationToken cancellationToken)
//         {
//             var message = new byte[1 + payload.Length];
//             message[0] = (byte)type;
//             payload.CopyTo(message, 1);
//             await SendMessageAsync(message, cancellationToken);
//         }
//
//         private async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken)
//         {
//             if (_stream == null)
//                 throw new InvalidOperationException("Not connected");
//
//             var lengthBytes = new byte[4];
//             var bytesRead = await _stream.ReadAsync(lengthBytes, cancellationToken);
//             if (bytesRead < 4)
//                 throw new IOException("Connection closed unexpectedly");
//
//             var length = BitConverter.ToInt32(lengthBytes);
//             var message = new byte[length];
//             var totalRead = 0;
//
//             while (totalRead < length)
//             {
//                 bytesRead = await _stream.ReadAsync(message.AsMemory(totalRead, length - totalRead), cancellationToken);
//                 if (bytesRead == 0)
//                     throw new IOException("Connection closed unexpectedly");
//                 totalRead += bytesRead;
//             }
//
//             return message;
//         }
//
//         public void Disconnect()
//         {
//             _stream?.Dispose();
//             _stream = null;
//
//             _tcpClient?.Dispose();
//             _tcpClient = null;
//
//             _secureChannel?.Dispose();
//             _secureChannel = null;
//
//             _ecdhKeyPair?.Dispose();
//             _ecdhKeyPair = null;
//
//             if (_sharedSecret != null)
//             {
//                 CryptographicOperations.ZeroMemory(_sharedSecret);
//                 _sharedSecret = null;
//             }
//
//             _connectedDevice = null;
//         }
//
//         #endregion
//
//         #region Message Creation
//
//         private static byte[] CreatePairingRequest(byte[] ecdhPublicKey)
//         {
//             using var ms = new MemoryStream();
//             using var writer = new BinaryWriter(ms);
//
//             writer.Write((byte)MessageType.PairingRequest);
//             writer.Write(ecdhPublicKey.Length);
//             writer.Write(ecdhPublicKey);
//
//             return ms.ToArray();
//         }
//
//         private static byte[] ParsePairingResponse(byte[] data)
//         {
//             using var ms = new MemoryStream(data);
//             using var reader = new BinaryReader(ms);
//
//             reader.ReadByte(); // Skip message type
//             var keyLength = reader.ReadInt32();
//             return reader.ReadBytes(keyLength);
//         }
//
//         private static byte[] CreatePairingConfirmMessage(string credentialId, string vaultName, string pairingId)
//         {
//             using var ms = new MemoryStream();
//             using var writer = new BinaryWriter(ms);
//
//             writer.Write((byte)MessageType.PairingConfirm);
//             writer.Write(credentialId);
//             writer.Write(vaultName);
//             writer.Write(pairingId);
//
//             return ms.ToArray();
//         }
//
//         private static byte[] ParsePairingComplete(byte[] data)
//         {
//             using var ms = new MemoryStream(data);
//             using var reader = new BinaryReader(ms);
//
//             reader.ReadByte(); // Skip message type
//             var keyLength = reader.ReadInt32();
//             return reader.ReadBytes(keyLength);
//         }
//
//         private static byte[] CreateSecureSessionRequest(string pairingId, byte[] nonce)
//         {
//             using var ms = new MemoryStream();
//             using var writer = new BinaryWriter(ms);
//
//             writer.Write((byte)MessageType.SecureSessionRequest);
//             writer.Write(pairingId);
//             writer.Write(nonce.Length);
//             writer.Write(nonce);
//
//             return ms.ToArray();
//         }
//
//         private static byte[] ParseSecureSessionAccepted(byte[] data)
//         {
//             using var ms = new MemoryStream(data);
//             using var reader = new BinaryReader(ms);
//
//             reader.ReadByte(); // Skip message type
//             var nonceLength = reader.ReadInt32();
//             return reader.ReadBytes(nonceLength);
//         }
//
//         private static byte[] CreateSecureAuthRequest(string credentialId, byte[] challenge, long timestamp,
//             byte[] nonce)
//         {
//             using var ms = new MemoryStream();
//             using var writer = new BinaryWriter(ms);
//
//             writer.Write(credentialId);
//             writer.Write(challenge.Length);
//             writer.Write(challenge);
//             writer.Write(timestamp);
//             writer.Write(nonce.Length);
//             writer.Write(nonce);
//
//             return ms.ToArray();
//         }
//
//         private static byte[] BuildSignedData(string credentialId, byte[] challenge, long timestamp, byte[] nonce)
//         {
//             using var ms = new MemoryStream();
//             using var writer = new BinaryWriter(ms);
//
//             writer.Write(Encoding.UTF8.GetBytes(credentialId));
//             writer.Write(challenge);
//             writer.Write(timestamp);
//             writer.Write(nonce);
//
//             return ms.ToArray();
//         }
//
//         private static string ParseAuthenticationRejected(byte[] data)
//         {
//             if (data.Length <= 1)
//                 return "Unknown reason";
//
//             using var ms = new MemoryStream(data);
//             using var reader = new BinaryReader(ms);
//
//             reader.ReadByte(); // Skip message type
//             return reader.ReadString();
//         }
//
//         #endregion
//
//         public void Dispose()
//         {
//             if (_disposed) return;
//             _disposed = true;
//
//             Disconnect();
//             _discovery.Dispose();
//             _configStore.Dispose();
//         }
//     }
// }