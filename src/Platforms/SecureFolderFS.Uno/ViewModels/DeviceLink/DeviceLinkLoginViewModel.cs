using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Uno.DataModels;

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public sealed partial class DeviceLinkLoginViewModel(IFolder vaultFolder, string vaultId)
        : DeviceLinkViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dataModel = await GetConfigurationAsync(cancellationToken);
                var keyResult = await AcquireAsync(VaultId, dataModel.Challenge, cancellationToken);
                if (!keyResult.TryGetValue(out var key))
                {
                    Report(keyResult);
                    return;
                }
                
                // Report that credentials were provided
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(key, tcs));

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }

        /// <inheritdoc/>
        protected override async Task<VaultDeviceLinkDataModel> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var auth = await vaultReader.ReadAuthenticationAsync<VaultDeviceLinkDataModel>($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            if (auth?.CredentialId is null || auth.MobileDeviceId is null || auth.ExpectedHmac is null || auth.Challenge is null)
                throw new FormatException("Invalid device link configuration.");

            return auth;
        }

        /// <inheritdoc/>
        protected override Task<bool> ShowVerificationCodeAsync(string code)
        {
            throw new NotSupportedException();
        }
    }
}
