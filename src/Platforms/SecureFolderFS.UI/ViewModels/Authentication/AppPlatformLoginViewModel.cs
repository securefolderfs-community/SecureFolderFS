using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
#if APP_PLATFORM_PRESENT
using SecureFolderFS.Sdk.AppPlatform;
using SecureFolderFS.Sdk.AppPlatform.Helpers;
using SecureFolderFS.Sdk.AppPlatform.Services;
#endif

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <summary>
    /// The system browser authenticates via Keycloak, decrypts the vault key
    /// client-side, and passes the result to a localhost callback.
    /// </summary>
    public sealed partial class AppPlatformLoginViewModel : AuthenticationViewModel
    {
        private readonly IFolder _vaultFolder;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.FirstStageOnly;

        public AppPlatformLoginViewModel(IFolder vaultFolder)
            : base(Core.Constants.Vault.Authentication.AUTH_APP_PLATFORM)
        {
            _vaultFolder = vaultFolder;
            Title = "App Platform";
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
#if APP_PLATFORM_PRESENT
            await ProvideCredentialsNativeAsync(cancellationToken);
#else
            await Task.FromException(new PlatformNotSupportedException(
                "App Platform authentication requires the SecureFolderFS.Sdk.AppPlatform project."));
#endif
        }

#if APP_PLATFORM_PRESENT
        /// <summary>
        /// Native flow: OIDC auth via system browser, then decrypt the vault key.
        /// If this is the first unlock on this device, prompts for the Account Key passphrase to bootstrap the device key chain.
        /// </summary>
        private async Task ProvideCredentialsNativeAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(_vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);

            if (config.AppPlatform is null)
                throw new InvalidOperationException("Vault is not configured for App Platform.");

            var serverUrl = config.AppPlatform.ServerUrl.TrimEnd('/');
            var vaultId = config.Uid;

            var authProvider = DI.Service<IOidcProvider>();
            var deviceKeyStore = DI.Service<IDeviceKeyStore>();

            using var client = new AppPlatformClient(serverUrl);
            var authConfig = await client.GetAuthConfigAsync(cancellationToken);
            var accessToken = await authProvider.GetAccessTokenAsync(
                authConfig.Authority, authConfig.ClientId, authConfig.Scopes, cancellationToken);
            client.SetAccessToken(accessToken);

            var keyManager = new AppPlatformKeyManager(deviceKeyStore, client, authProvider);

            // If no device is registered on this machine, bootstrap one using the Account Key passphrase
            if (!await deviceKeyStore.HasPrivateKeyAsync(cancellationToken))
            {
                StateChanged?.Invoke(this, EventArgs.Empty);

                var overlayService = DI.Service<IOverlayService>();
                var overlay = new DeviceSetupOverlayViewModel();
                var result = await overlayService.ShowAsync(overlay);

                // User requested an account key reset instead of providing a passphrase
                if (overlay.ResetRequested)
                {
                    await client.RequestKeyResetAsync(cancellationToken);
                    throw new OperationCanceledException(
                        "Account key reset requested. An administrator must approve your request before you can set up this device again.");
                }

                if (!result.Successful || string.IsNullOrEmpty(overlay.Passphrase))
                    throw new OperationCanceledException("Account Key passphrase is required to set up this device.");

                var deviceName = Environment.MachineName;
                await keyManager.BootstrapDeviceAsync(deviceName, overlay.Passphrase, cancellationToken);
            }

            var (dekKey, macKey) = await keyManager.DecryptVaultKeyAsync(vaultId, cancellationToken);

            var combined = new byte[dekKey.Length + macKey.Length];
            try
            {
                Array.Copy(dekKey, 0, combined, 0, dekKey.Length);
                Array.Copy(macKey, 0, combined, dekKey.Length, macKey.Length);

                var key = ManagedKey.TakeOwnership(combined);
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));
                await tcs.Task;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(dekKey);
                CryptographicOperations.ZeroMemory(macKey);
            }
        }
#endif
    }
}
