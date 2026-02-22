using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.DeviceLink.Results;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Uno.DataModels;

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public sealed partial class DeviceLinkCreationViewModel(IFolder vaultFolder, string vaultId)
        : DeviceLinkViewModel(vaultFolder, vaultId)
    {
        private TaskCompletionSource<bool>? _verificationCodeTcs;
        
        [ObservableProperty] private string? _VerificationCode;
        
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var challenge = CryptHelpers.GenerateChallenge(VaultId, Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH_32);

                var keyResult = await EnrollAsync(VaultId, challenge.Key, cancellationToken);
                if (!keyResult.TryGetValue(out var key) || keyResult is not DeviceLinkPairingResult pairingResult)
                {
                    Report(keyResult);
                    return;
                }

                var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
                await vaultWriter.WriteAuthenticationAsync<VaultDeviceLinkDataModel>(
                    $"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
                    {
                        Capability = "supportsChallenge|supportsEcdhTransport|supportsQuantum",
                        Challenge = challenge.Key,
                        PairingId = pairingResult.PairingId,
                        CredentialId = pairingResult.CredentialId,
                        MobileDeviceId = pairingResult.MobileDeviceId,
                        MobileDeviceName = pairingResult.MobileDeviceName,
                        MobileDeviceType = pairingResult.MobileDeviceType,
                        ExpectedHmac = key.Key,
                        CreatedAt = DateTime.Now
                    }, cancellationToken);
                
                // Report that credentials were provided
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }

        /// <inheritdoc/>
        protected override Task<bool> ShowVerificationCodeAsync(string code)
        {
            _verificationCodeTcs?.TrySetCanceled();
            _verificationCodeTcs = new();
            VerificationCode = code;

            return _verificationCodeTcs.Task;
        }
        
        /// <inheritdoc/>
        protected override Task<VaultDeviceLinkDataModel> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
        
        [RelayCommand]
        private void ConfirmVerificationCode()
        {
            VerificationCode = null;
            _verificationCodeTcs?.TrySetResult(true);
        }
        
        [RelayCommand]
        private void RejectVerificationCode()
        {
            VerificationCode = null;
            _verificationCodeTcs?.TrySetResult(false);
        }
    }
}
