using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.Uno.DataModels;

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public sealed partial class DeviceLinkLoginViewModel(IFolder vaultFolder, string vaultId)
        : DeviceLinkViewModel(vaultFolder, vaultId), IAsyncInitialize
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var dataModel = await GetConfigurationAsync(cancellationToken);
                var mediaService = DI.Service<IMediaService>();

                Icon = await mediaService.GetImageFromResourceAsync($"{dataModel.MobileDeviceType}_Device", cancellationToken);
            }
            catch (Exception)
            {
                Icon = new ImageGlyph("\uE8EA");
            }
        }
        
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
