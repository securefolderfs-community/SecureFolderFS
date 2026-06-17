using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
#if APP_PLATFORM_PRESENT
using SecureFolderFS.Sdk.AppPlatform;
using SecureFolderFS.Sdk.AppPlatform.Dto;
using SecureFolderFS.Sdk.AppPlatform.Helpers;
using SecureFolderFS.Sdk.AppPlatform.Services;
using static SecureFolderFS.Core.Constants.Vault.Authentication;
#endif

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <summary>
    /// The system browser authenticates via Keycloak, decrypts the vault key
    /// client-side, and passes the result to a localhost callback.
    /// </summary>
    public sealed partial class AppPlatformLoginViewModel : AuthenticationViewModel, IAsyncInitialize
    {
        private readonly IFolder _vaultFolder;
        private string? _serverUrl;
        private string? _vaultId;

        [ObservableProperty] private AccountItemViewModel? _SelectedAccount;

        /// <summary>
        /// Gets the accounts the user can choose from when logging in, including a "new account" option.
        /// </summary>
        public ObservableCollection<AccountItemViewModel> Accounts { get; } = new();

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.FirstStageOnly;

        public AppPlatformLoginViewModel(IFolder vaultFolder)
            : base(AUTH_APP_PLATFORM)
        {
            _vaultFolder = vaultFolder;
            Title = "App Platform";
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
#if APP_PLATFORM_PRESENT
            var vaultReader = new VaultReader(_vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            if (config.AppPlatform is null)
                return;

            _serverUrl = config.AppPlatform.ServerUrl.TrimEnd('/');
            _vaultId = config.Uid;

            Accounts.Clear();
            var newAccountOption = new AccountItemViewModel(new AccountModel(string.Empty, "Use a new account", null, null, AUTH_APP_PLATFORM));
            Accounts.Add(newAccountOption);

            var deviceKeyStore = DI.Service<IDeviceKeyStore>();
            var mediaService = DI.Service<IMediaService>();
            
            var icon = await mediaService.GetImageFromResourceAsync("AppPlatformIcon", cancellationToken);
            var normalizedServer = AppPlatformEndpointGuard.NormalizeServerUrl(_serverUrl);
            foreach (var account in await deviceKeyStore.GetAccountsAsync(cancellationToken))
            {
                // Only offer accounts that belong to this vault's server (or whose server is unknown).
                if (account.ServerUrl is not null &&
                    !string.Equals(AppPlatformEndpointGuard.NormalizeServerUrl(account.ServerUrl), normalizedServer, StringComparison.OrdinalIgnoreCase))
                    continue;

                Accounts.Add(new AccountItemViewModel(new AccountModel(account.Id, account.DisplayName ?? account.Id, account.ServerUrl, icon, AUTH_APP_PLATFORM)));
            }

            SelectedAccount = Accounts.Count > 1 ? Accounts[1] : newAccountOption;
#else
            await Task.CompletedTask;
#endif
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
            try
            {
                await ProvideCredentialsNativeAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The user canceled the in-progress browser sign-in (e.g. closed the browser).
                // Treat it as a no-op so the command resets and the Authenticate button re-enables.
            }
#else
            await Task.FromException(new NotSupportedException("App Platform authentication requires the SecureFolderFS.Sdk.AppPlatform project."));
#endif
        }

#if APP_PLATFORM_PRESENT
        /// <summary>
        /// Native flow: OIDC auth via system browser, then decrypt the vault key.
        /// The user may pick an existing account (reusing its device key) or set up a new one,
        /// in which case the Account Key passphrase bootstraps a fresh device key chain.
        /// </summary>
        private async Task ProvideCredentialsNativeAsync(CancellationToken cancellationToken)
        {
            // Ensure configuration is loaded even if InitAsync was skipped.
            if (_serverUrl is null || _vaultId is null)
                await InitAsync(cancellationToken);

            var serverUrl = _serverUrl ?? throw new InvalidOperationException("Vault is not configured for App Platform.");
            var vaultId = _vaultId!;

            var authProvider = DI.Service<IOidcProvider>();
            var deviceKeyStore = DI.Service<IDeviceKeyStore>();

            // Resolve the picker selection up-front. A null/empty selection means "use a new account".
            var selectedAccountId = SelectedAccount?.Id;
            var isNewAccount = string.IsNullOrEmpty(selectedAccountId);

            using var client = new AppPlatformClient(serverUrl);
            var authConfig = await client.GetAuthConfigAsync(cancellationToken);

            // For a new account, force a fresh Keycloak login so the user can pick a different identity
            // instead of silently reusing the existing SSO session.
            var accessToken = await authProvider.GetAccessTokenAsync(
                authConfig.Authority, authConfig.ClientId, authConfig.Scopes, forceLogin: isNewAccount, cancellationToken: cancellationToken);
            client.SetAccessToken(accessToken);

            UserInfoDto? user = null;
            string accountId;

            if (!isNewAccount)
            {
                accountId = selectedAccountId!;
            }
            else
            {
                // "Use a new account": resolve the signed-in identity first, so re-authenticating as
                // an already-known user reuses that account instead of creating a duplicate.
                user = await client.GetMeAsync(cancellationToken);
                var normalizedServer = AppPlatformEndpointGuard.NormalizeServerUrl(serverUrl);
                var existing = (await deviceKeyStore.GetAccountsAsync(cancellationToken)).FirstOrDefault(a =>
                    a.UserId == user.Id &&
                    (a.ServerUrl is null ||
                     string.Equals(AppPlatformEndpointGuard.NormalizeServerUrl(a.ServerUrl), normalizedServer, StringComparison.OrdinalIgnoreCase)));

                accountId = existing?.Id ?? Guid.NewGuid().ToString();
            }

            var keyManager = new AppPlatformKeyManager(deviceKeyStore, client, authProvider, accountId);

            // Bootstrap a device key for this account if it doesn't have one yet.
            if (!await deviceKeyStore.HasPrivateKeyAsync(accountId, cancellationToken))
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

                // Persist account metadata so it can be reused (and managed) next time.
                user ??= await client.GetMeAsync(cancellationToken);
                var displayName = user.Email ?? user.DisplayName ?? user.Id;
                await deviceKeyStore.SetAccountAsync(
                    new DeviceKeyAccount { Id = accountId, DisplayName = displayName, ServerUrl = serverUrl, UserId = user.Id },
                    cancellationToken);
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
