using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using Yubico.YubiKey;
using Yubico.YubiKey.Otp;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    [Bindable(true)]
    public abstract partial class YubiKeyViewModel : AuthenticationViewModel
    {
        private readonly SynchronizationContext? _synchronizationContext;

        [ObservableProperty] private bool _IsAwaitingTouch;
        [ObservableProperty] private bool _UseLongPress = true;
        
        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        protected string VaultId { get; }

        /// <summary>
        /// Gets the associated folder of the vault.
        /// </summary>
        protected IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public sealed override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public sealed override AuthenticationStage Availability { get; } = AuthenticationStage.Any;

        protected YubiKeyViewModel(IFolder vaultFolder, string vaultId)
            : base(Core.Constants.Vault.Authentication.AUTH_YUBIKEY)
        {
            _synchronizationContext = SynchronizationContext.Current;
            Title = "YubiKey".ToLocalized();
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            if (VaultFolder is not IModifiableFolder modifiableFolder)
                return;

            var authenticationFile = await modifiableFolder.TryGetFileByNameAsync($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            if (authenticationFile is null)
                return;
            
            await modifiableFolder.DeleteAsync(authenticationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKeyBytes> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            try
            {
                // During enrollment, configure the slot for HMAC-SHA1 challenge-response first
                return await PerformChallengeResponseAsync(
                    data, 
                    configureSlot: true, 
                    UseLongPress,
                    () => _synchronizationContext.PostOrExecute(_ => IsAwaitingTouch = true),
                    cancellationToken);
            }
            finally
            {
                IsAwaitingTouch = false;
            }
        }

        /// <inheritdoc/>
        public override async Task<IKeyBytes> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            try
            {
                // During login, the slot should already be configured
                // UseLongPress should be set from the saved configuration before calling this
                return await PerformChallengeResponseAsync(
                    data, 
                    configureSlot: false, 
                    UseLongPress,
                    () => _synchronizationContext.PostOrExecute(_ => IsAwaitingTouch = true),
                    cancellationToken);
            }
            finally
            {
                IsAwaitingTouch = false;
            }
        }

        /// <summary>
        /// Finds the first available YubiKey device.
        /// </summary>
        /// <returns>The found YubiKey device.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no YubiKey is found.</exception>
        protected static IYubiKeyDevice GetYubiKeyDevice()
        {
            var yubiKeyDevices = YubiKeyDevice.FindAll();
            var device = yubiKeyDevices.FirstOrDefault();

            return device ??
                   throw new InvalidOperationException(
                       "No YubiKey device found. Please insert your YubiKey and try again.");
        }

        /// <summary>
        /// Performs challenge-response authentication using the YubiKey's HMAC-SHA1 slot.
        /// </summary>
        /// <param name="challenge">The challenge data to send to the YubiKey.</param>
        /// <param name="configureSlot">If true, configures the slot for HMAC-SHA1 before performing challenge-response.</param>
        /// <param name="useLongPress">If true, uses Slot 2 (LongPress); otherwise uses Slot 1 (ShortPress).</param>
        /// <param name="touchNotifier">Callback invoked when touch is required.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response key bytes.</returns>
        protected static Task<ManagedKey> PerformChallengeResponseAsync(
            byte[] challenge, 
            bool configureSlot,
            bool useLongPress,
            Action? touchNotifier,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var device = GetYubiKeyDevice();
                using var otpSession = new OtpSession(device);

                // YubiKey HMAC-SHA1 challenge-response accepts up to 64 bytes.
                // Hash the full challenge with SHA-512 to produce exactly 64 bytes
                // while preserving all entropy from the original challenge.
                var challengeBytes = SHA512.HashData(challenge);

                // Determine which slot to use based on user preference
                var slot = useLongPress ? Slot.LongPress : Slot.ShortPress;

                if (configureSlot)
                    ConfigureSlotForChallengeResponse(otpSession, slot);

                // Perform challenge-response
                var response = ChallengeResponse(otpSession, slot, challengeBytes, touchNotifier);
                
                if (response is null)
                    throw new InvalidOperationException("Challenge-response failed. Ensure the YubiKey slot is configured for HMAC-SHA1.");

                var responseBytes = response.Value.ToArray();
                var secretKey = new ManagedKey(responseBytes.Length);
                Array.Copy(responseBytes, secretKey.Key, responseBytes.Length);

                return secretKey;
            }, cancellationToken);
        }

        /// <summary>
        /// Configures a YubiKey slot for HMAC-SHA1 challenge-response with touch required.
        /// </summary>
        /// <param name="session">The OTP session.</param>
        /// <param name="slot">The slot to configure.</param>
        /// <remarks>
        /// This will overwrite any existing configuration in the specified slot.
        /// The generated HMAC-SHA1 key is stored securely on the YubiKey and cannot be extracted.
        /// </remarks>
        private static void ConfigureSlotForChallengeResponse(IOtpSession session, Slot slot)
        {
            var key = new byte[Core.Cryptography.Constants.KeyTraits.HMAC_SHA1_HASH_LENGTH];

            try
            {
                session.ConfigureChallengeResponse(slot)
                    .UseHmacSha1()
                    .UseButton() // Require touch for security
                    .GenerateKey(key) // Generate a random key (stored on YubiKey)
                    .Execute();

                Debug.WriteLine($"Successfully configured {slot} for HMAC-SHA1 challenge-response with touch required.");
            }
            finally
            {
                // Clear the key from memory (the real key is on the YubiKey)
                CryptographicOperations.ZeroMemory(key);
            }
        }

        /// <summary>
        /// Performs challenge-response on a specific slot.
        /// </summary>
        /// <param name="session">The OTP session.</param>
        /// <param name="slot">The slot to use.</param>
        /// <param name="challenge">The challenge bytes (must be exactly 64 bytes).</param>
        /// <param name="touchNotifier">Callback invoked when touch is required.</param>
        /// <returns>The response bytes, or null if the operation failed.</returns>
        private static ReadOnlyMemory<byte>? ChallengeResponse(
            IOtpSession session, 
            Slot slot, 
            byte[] challenge,
            Action? touchNotifier)
        {
            if (challenge.Length != Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH_64)
                throw new ArgumentException("Challenge must be exactly 64 bytes.", nameof(challenge));

            try
            {
                // Perform HMAC-SHA1 challenge-response.
                // UseYubiOtp(false) ensures we're using HMAC-SHA1 mode, not Yubico OTP.
                // UseTouchNotifier provides a callback when touch is required.
                // The SDK will wait for the user to touch the key after the callback fires.
                return session.CalculateChallengeResponse(slot)
                    .UseChallenge(challenge)
                    .UseYubiOtp(false)
                    .UseTouchNotifier(() =>
                    {
                        Debug.WriteLine($"YubiKey requires touch on {slot}. Please touch your YubiKey now.");
                        touchNotifier?.Invoke();
                    })
                    .GetDataBytes();
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("keyboard", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("acknowledge", StringComparison.OrdinalIgnoreCase))
            {
                // This error occurs when:
                // 1. The slot is not configured for HMAC-SHA1 challenge-response
                // 2. The slot is configured for a different mode (static password, Yubico OTP, HOTP)
                // 3. There's a USB communication issue
                Debug.WriteLine($"Slot {slot} failed - likely not configured for HMAC-SHA1 challenge-response: {ex.Message}");
                return null;
            }
            catch (TimeoutException ex)
            {
                // Touch timeout - user didn't touch the key in time
                Debug.WriteLine($"Challenge-response timed out on {slot}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Log the full exception type and inner exception for debugging
                Debug.WriteLine($"Challenge-response failed on {slot} ({ex.GetType().FullName}): {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"  Inner exception ({ex.InnerException.GetType().FullName}): {ex.InnerException.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a YubiKey device is available.
        /// </summary>
        /// <returns>True if a YubiKey is found, false otherwise.</returns>
        public static bool IsSupported()
        {
            try
            {
                var devices = YubiKeyDevice.FindAll();
                return devices.Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if challenge-response is likely configured on any slot.
        /// Note: This only checks if slots are configured, not specifically for HMAC-SHA1 mode.
        /// The actual mode can only be determined by attempting the operation.
        /// </summary>
        /// <returns>True if at least one slot is configured, false otherwise.</returns>
        public static bool HasConfiguredSlot()
        {
            try
            {
                var device = GetYubiKeyDevice();
                using var otpSession = new OtpSession(device);
                return otpSession.IsShortPressConfigured || otpSession.IsLongPressConfigured;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets diagnostic information about the YubiKey's OTP slot configuration.
        /// </summary>
        /// <returns>A string containing diagnostic information.</returns>
        public static string GetDiagnosticInfo()
        {
            try
            {
                var device = GetYubiKeyDevice();
                using var otpSession = new OtpSession(device);
                
                return $"YubiKey: {device.SerialNumber}\n" +
                       $"Firmware: {device.FirmwareVersion}\n" +
                       $"Slot 1 (ShortPress) configured: {otpSession.IsShortPressConfigured}\n" +
                       $"Slot 2 (LongPress) configured: {otpSession.IsLongPressConfigured}\n" +
                       $"Note: 'Configured' only means something is in the slot, not that it's HMAC-SHA1.\n" +
                       $"To configure for challenge-response, use: ykman otp chalresp --touch --generate 2";
            }
            catch (Exception ex)
            {
                return $"Failed to get diagnostic info: {ex.Message}";
            }
        }
    }
}
