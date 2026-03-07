using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Uno.Views.DebugViews
{
    /// <summary>
    /// Debug page that simulates Phone Link authentication flow.
    /// This demonstrates BLE discovery and challenge-response authentication
    /// that would be used between desktop and mobile versions of SecureFolderFS.
    /// </summary>
    public sealed partial class DebugPhoneLinkPage : Page
    {
        // Simulated mobile device state
        private ECDsa? _mobilePrivateKey;
        private byte[]? _mobilePublicKey;
        private bool _isAdvertising;

        // Simulated desktop state
        private byte[]? _storedPublicKey;
        private byte[]? _currentChallenge;
        private byte[]? _lastSignature;
        private bool _isConnected;

        // Simulated discovered devices
        private readonly ObservableCollection<SimulatedDevice> _discoveredDevices = new();

        public DebugPhoneLinkPage()
        {
            InitializeComponent();
            DiscoveredDevicesList.ItemsSource = _discoveredDevices;
        }

        #region Mobile Device Simulation

        private void GenerateKeypair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dispose old key if exists
                _mobilePrivateKey?.Dispose();

                // Generate new ECDSA keypair using P-256 curve
                _mobilePrivateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                _mobilePublicKey = _mobilePrivateKey.ExportSubjectPublicKeyInfo();

                // Update UI
                PublicKeyDisplay.Text = Convert.ToBase64String(_mobilePublicKey);
                KeypairStatus.Text = $"Keypair generated (P-256, {_mobilePublicKey.Length} bytes)";
                KeypairStatusIcon.Glyph = "\uE73E"; // Checkmark
                KeypairStatusIcon.Foreground = new SolidColorBrush(Colors.Green);

                // Enable advertising toggle
                AdvertisingToggle.IsEnabled = true;

                Log($"[MOBILE] Generated new ECDSA P-256 keypair");
                Log($"[MOBILE] Public key: {Convert.ToBase64String(_mobilePublicKey)[..40]}...");
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Failed to generate keypair: {ex.Message}");
            }
        }

        private void AdvertisingToggle_Toggled(object sender, RoutedEventArgs e)
        {
            _isAdvertising = AdvertisingToggle.IsOn;

            if (_isAdvertising)
            {
                AdvertisingIcon.Glyph = "\uEC3F"; // Bluetooth icon
                AdvertisingIcon.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                Log($"[MOBILE] Started BLE advertising as '{MobileDeviceName.Text}'");
                Log($"[MOBILE] Service UUID: a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            }
            else
            {
                AdvertisingIcon.Foreground = new SolidColorBrush(Colors.Gray);
                Log($"[MOBILE] Stopped BLE advertising");
            }
        }

        /// <summary>
        /// Simulates the mobile device signing a challenge from the desktop.
        /// </summary>
        private byte[] MobileSignChallenge(byte[] challenge)
        {
            if (_mobilePrivateKey is null)
                throw new InvalidOperationException("No keypair available on mobile device");

            // Sign the challenge using ECDSA with SHA-256
            var signature = _mobilePrivateKey.SignData(challenge, HashAlgorithmName.SHA256);

            Log($"[MOBILE] Received challenge: {Convert.ToHexString(challenge)}");
            Log($"[MOBILE] Signed with private key, signature: {Convert.ToHexString(signature)[..40]}...");

            return signature;
        }

        #endregion

        #region Desktop Client Simulation

        private async void Discover_Click(object sender, RoutedEventArgs e)
        {
            DiscoverButton.IsEnabled = false;
            DiscoverButton.Content = "🔍 Scanning...";
            _discoveredDevices.Clear();

            Log($"[DESKTOP] Starting BLE scan for SecureFolderFS devices...");

            // Simulate scanning delay
            await Task.Delay(1500);

            // Check if mobile is advertising
            if (_isAdvertising && _mobilePublicKey is not null)
            {
                var device = new SimulatedDevice
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = MobileDeviceName.Text,
                    SignalStrength = $"-{Random.Shared.Next(40, 70)} dBm",
                    PublicKey = _mobilePublicKey
                };

                _discoveredDevices.Add(device);
                Log($"[DESKTOP] Found device: {device.Name} (Signal: {device.SignalStrength})");
            }
            else
            {
                Log($"[DESKTOP] No devices found. Make sure mobile is advertising.");
            }

            DiscoverButton.Content = "🔍 Scan for Devices";
            DiscoverButton.IsEnabled = true;
        }

        private void DiscoveredDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DiscoveredDevicesList.SelectedItem is not SimulatedDevice device)
                return;

            // Simulate connection
            _isConnected = true;
            _storedPublicKey = device.PublicKey;

            ConnectionStatus.Text = $"Connected to {device.Name}";
            ConnectionStatusIcon.Glyph = "\uE703"; // Connected icon
            ConnectionStatusIcon.Foreground = new SolidColorBrush(Colors.Green);

            // Enable challenge-response controls
            GenerateChallengeButton.IsEnabled = true;

            Log($"[DESKTOP] Connecting to {device.Name}...");
            Log($"[DESKTOP] Performing ECDH key exchange for secure channel...");
            Log($"[DESKTOP] Secure channel established (AES-256-GCM)");
            Log($"[DESKTOP] Stored device public key for future verification");
        }

        private void GenerateChallenge_Click(object sender, RoutedEventArgs e)
        {
            // Generate random 32-byte challenge
            _currentChallenge = new byte[32];
            RandomNumberGenerator.Fill(_currentChallenge);

            ChallengeInput.Text = Convert.ToHexString(_currentChallenge);
            SendChallengeButton.IsEnabled = true;

            Log($"[DESKTOP] Generated challenge: {Convert.ToHexString(_currentChallenge)}");
        }

        private void SendChallenge_Click(object sender, RoutedEventArgs e)
        {
            if (_currentChallenge is null || !_isConnected)
                return;

            try
            {
                Log($"[DESKTOP] Sending challenge to mobile over secure channel...");

                // Simulate sending challenge and receiving signed response
                _lastSignature = MobileSignChallenge(_currentChallenge);
                ResponseDisplay.Text = Convert.ToHexString(_lastSignature);

                // Verify the signature using stored public key
                if (_storedPublicKey is not null)
                {
                    using var verifyKey = ECDsa.Create();
                    verifyKey.ImportSubjectPublicKeyInfo(_storedPublicKey, out _);

                    var isValid = verifyKey.VerifyData(_currentChallenge, _lastSignature, HashAlgorithmName.SHA256);

                    if (isValid)
                    {
                        VerificationStatus.Text = "✓ Signature verified successfully";
                        VerificationIcon.Glyph = "\uE73E";
                        VerificationIcon.Foreground = new SolidColorBrush(Colors.Green);
                        DeriveKeyButton.IsEnabled = true;

                        Log($"[DESKTOP] Signature verification: PASSED");
                    }
                    else
                    {
                        VerificationStatus.Text = "✗ Signature verification failed";
                        VerificationIcon.Glyph = "\uE711";
                        VerificationIcon.Foreground = new SolidColorBrush(Colors.Red);

                        Log($"[DESKTOP] Signature verification: FAILED");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Challenge-response failed: {ex.Message}");
            }
        }

        private void DeriveKey_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSignature is null || _currentChallenge is null)
                return;

            try
            {
                // Derive a key from the signature using HKDF
                // This key would be used to decrypt the vault's keystore
                var info = Encoding.UTF8.GetBytes("SecureFolderFS-PhoneLink-VaultKey-v1");
                var derivedKey = DeriveKeyFromSignature(_lastSignature, _currentChallenge, info, 32);

                DerivedKeyDisplay.Text = Convert.ToHexString(derivedKey);

                Log($"[DESKTOP] Derived vault decryption key using HKDF-SHA256");
                Log($"[DESKTOP] Key: {Convert.ToHexString(derivedKey)}");
                Log($"[DESKTOP] This key can now decrypt the vault's keystore file");

                // Clean up
                Array.Clear(derivedKey);
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Key derivation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Derives a cryptographic key from the signature using HKDF.
        /// </summary>
        private static byte[] DeriveKeyFromSignature(byte[] signature, byte[] salt, byte[] info, int keyLength)
        {
            // Use HKDF to derive a key from the signature
            // The signature serves as the input key material (IKM)
            // The challenge serves as salt
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, signature, keyLength, salt, info);
        }

        #endregion

        #region Full Flow Demo

        private async void RunFullFlow_Click(object sender, RoutedEventArgs e)
        {
            Log("\n========== FULL AUTHENTICATION FLOW ==========\n");

            // Step 1: Mobile generates keypair
            Log("[STEP 1] Mobile device enrollment...");
            GenerateKeypair_Click(sender, e);
            await Task.Delay(500);

            // Step 2: Mobile starts advertising
            Log("\n[STEP 2] Mobile starts BLE advertising...");
            AdvertisingToggle.IsOn = true;
            await Task.Delay(500);

            // Step 3: Desktop scans
            Log("\n[STEP 3] Desktop scans for devices...");
            Discover_Click(sender, e);
            await Task.Delay(2000);

            // Step 4: Desktop connects
            Log("\n[STEP 4] Desktop connects to device...");
            if (_discoveredDevices.Count > 0)
            {
                DiscoveredDevicesList.SelectedIndex = 0;
                await Task.Delay(500);
            }

            // Step 5: Challenge-response
            Log("\n[STEP 5] Challenge-response authentication...");
            GenerateChallenge_Click(sender, e);
            await Task.Delay(300);
            SendChallenge_Click(sender, e);
            await Task.Delay(500);

            // Step 6: Derive key
            Log("\n[STEP 6] Derive vault decryption key...");
            DeriveKey_Click(sender, e);

            Log("\n========== FLOW COMPLETE ==========");
            Log("The derived key can now be used to decrypt the vault's keystore.");
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            ProtocolLog.Text += $"\n[{timestamp}] {message}";
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            ProtocolLog.Text = "Log cleared...";
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents a simulated BLE device for the demo.
        /// </summary>
        public class SimulatedDevice
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string SignalStrength { get; set; } = string.Empty;
            public byte[]? PublicKey { get; set; }
        }

        #endregion

        #region IKeyBytes Implementation for Integration

        /// <summary>
        /// Creates an IKeyBytes instance from the derived key for vault decryption.
        /// This demonstrates how the Phone Link authentication would integrate with
        /// the existing vault unlocking infrastructure.
        /// </summary>
        public static IKeyBytes CreateKeyBytesFromDerivedKey(byte[] derivedKey)
        {
            var managedKey = new ManagedKey(derivedKey.Length);
            Array.Copy(derivedKey, managedKey.Key, derivedKey.Length);
            return managedKey;
        }

        #endregion
    }
}

