using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;
using Yubico.YubiKey;
using Yubico.YubiKey.Otp;

namespace SecureFolderFS.Uno.ViewModels.YubiKey
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    [Bindable(true)]
    public abstract partial class YubiKeyViewModel : AuthenticationViewModel
    {
        private readonly SynchronizationContext? _synchronizationContext;
        private TaskCompletionSource<bool>? _slotOverwriteDecisionTcs;

        [ObservableProperty] private bool _IsAwaitingTouch;
        [ObservableProperty] private bool _UseLongPress = true;
        [ObservableProperty] private bool _IsSlotOverwriteWarningOpen;

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
            : base(Constants.Vault.Authentication.AUTH_YUBIKEY)
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

            var authenticationFile = await modifiableFolder.TryGetFileByNameAsync($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            if (authenticationFile is null)
                return;

            await modifiableFolder.DeleteAsync(authenticationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                Action touchNotifier = () => _synchronizationContext.PostOrExecute(_ => IsAwaitingTouch = true);

                // The slot is global YubiKey state shared by every vault and application, so reuse
                // an existing secret whenever possible; regenerating it would irreversibly break
                // everything else that relies on the slot
                var slotConfigured = await Task.Run(() => IsSlotConfigured(UseLongPress), cancellationToken);
                if (slotConfigured)
                {
                    var existingKey = await PerformChallengeResponseAsync(data, configureSlot: false, UseLongPress, touchNotifier, cancellationToken);
                    if (existingKey is not null)
                        return Result<IKeyBytes>.Success(existingKey);

                    // The slot holds a configuration other than HMAC-SHA1 challenge-response;
                    // overwriting it is destructive, so the user must explicitly confirm
                    _synchronizationContext.PostOrExecute(_ => IsAwaitingTouch = false);
                    if (!await RequestSlotOverwriteConfirmationAsync())
                        throw new OperationCanceledException("The user declined to overwrite the configured YubiKey slot.");
                }

                var key = await PerformChallengeResponseAsync(data, configureSlot: true, UseLongPress, touchNotifier, cancellationToken)
                          ?? throw new InvalidOperationException("Challenge-response failed after configuring the slot for HMAC-SHA1.");

                return Result<IKeyBytes>.Success(key);
            }
            finally
            {
                IsAwaitingTouch = false;
            }
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                // During login, the slot should already be configured
                // UseLongPress should be set from the saved configuration before calling this
                var key = await PerformChallengeResponseAsync(
                    data,
                    configureSlot: false,
                    UseLongPress,
                    () => _synchronizationContext.PostOrExecute(_ => IsAwaitingTouch = true),
                    cancellationToken)
                    ?? throw new InvalidOperationException("Challenge-response failed. Ensure the YubiKey slot is configured for HMAC-SHA1.");

                return Result<IKeyBytes>.Success(key);
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
        /// <returns>The response key bytes, or null when the slot is not configured for HMAC-SHA1 challenge-response.</returns>
        protected static Task<ManagedKey?> PerformChallengeResponseAsync(
            byte[] challenge,
            bool configureSlot,
            bool useLongPress,
            Action? touchNotifier,
            CancellationToken cancellationToken)
        {
            return Task.Run<ManagedKey?>(() =>
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
                    return null;

                var responseBytes = response.Value.ToArray();
                var secretKey = new ManagedKey(responseBytes.Length);
                Array.Copy(responseBytes, secretKey.Key, responseBytes.Length);

                return secretKey;
            }, cancellationToken);
        }

        /// <summary>
        /// Checks whether the given slot already holds a configuration of any kind.
        /// </summary>
        /// <param name="useLongPress">If true, checks Slot 2 (LongPress); otherwise Slot 1 (ShortPress).</param>
        /// <returns>True if the slot is configured, false otherwise.</returns>
        private static bool IsSlotConfigured(bool useLongPress)
        {
            var device = GetYubiKeyDevice();
            using var otpSession = new OtpSession(device);

            return useLongPress ? otpSession.IsLongPressConfigured : otpSession.IsShortPressConfigured;
        }

        /// <summary>
        /// Opens the slot overwrite warning and waits for the user's decision.
        /// </summary>
        /// <returns>True if the user confirmed overwriting the slot, false otherwise.</returns>
        private Task<bool> RequestSlotOverwriteConfirmationAsync()
        {
            _slotOverwriteDecisionTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _synchronizationContext.PostOrExecute(_ => IsSlotOverwriteWarningOpen = true);

            return _slotOverwriteDecisionTcs.Task;
        }

        partial void OnIsSlotOverwriteWarningOpenChanged(bool value)
        {
            // Dismissing the warning without choosing the action counts as declining
            if (!value)
                _slotOverwriteDecisionTcs?.TrySetResult(false);
        }

        [RelayCommand]
        private void ConfirmSlotOverwrite()
        {
            _slotOverwriteDecisionTcs?.TrySetResult(true);
            IsSlotOverwriteWarningOpen = false;
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
        /// <returns>The response bytes, or null when the slot is not configured for HMAC-SHA1 challenge-response.</returns>
        private static ReadOnlyMemory<byte>? ChallengeResponse(
            IOtpSession session,
            Slot slot,
            byte[] challenge,
            Action? touchNotifier)
        {
            if (challenge.Length != Core.Cryptography.Constants.KeyTraits.KEY_PART_LENGTH_64)
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
                // Thrown when the slot is configured for a mode other than HMAC-SHA1 challenge-response
                // (static password, Yubico OTP, HOTP). Only this case may return null - callers use it
                // to decide whether reprogramming the slot should be offered, so timeouts and transport
                // errors must propagate instead of being conflated with a misconfigured slot.
                Debug.WriteLine($"Slot {slot} is not configured for HMAC-SHA1 challenge-response: {ex.Message}");
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
