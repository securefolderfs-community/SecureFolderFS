using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Core.Cryptography.Jwe;
using SecureFolderFS.Sdk.AppPlatform;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    public sealed partial class AppPlatformCreationViewModel : AuthenticationViewModel, IVaultOptionsProvider, IAppPlatformVaultRegistration
    {
        private AppPlatformClient? _client;

        [ObservableProperty] private string? _ServerUrl;
        [ObservableProperty] private bool _IsAuthenticated;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.FirstStageOnly;

        public AppPlatformCreationViewModel()
            : base(Core.Constants.Vault.Authentication.AUTH_APP_PLATFORM)
        {
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
            if (string.IsNullOrWhiteSpace(ServerUrl))
                throw new InvalidOperationException("A server URL is required.");

            var authProvider = DI.Service<IAppPlatformAuthProvider>();

            _client?.Dispose();
            _client = new AppPlatformClient(ServerUrl);

            var authConfig = await _client.GetAuthConfigAsync(cancellationToken);
            var accessToken = await authProvider.GetAccessTokenAsync(
                authConfig.Authority, authConfig.ClientId, authConfig.Scopes, cancellationToken);
            _client.SetAccessToken(accessToken);

            var user = await _client.GetMeAsync(cancellationToken);
            if (!user.IsSetupComplete || string.IsNullOrWhiteSpace(user.PublicKeyJwk))
                throw new InvalidOperationException("Complete the App Platform first-time setup before creating a vault.");

            IsAuthenticated = true;

            var tcs = new TaskCompletionSource();
            CredentialsProvided?.Invoke(this, new(ManagedKey.Empty, tcs));
            await tcs.Task;
        }

        /// <inheritdoc/>
        public VaultOptions AmendVaultOptions(VaultOptions options)
        {
            return options with
            {
                AppPlatform = new AppPlatformVaultOptions
                {
                    ServerUrl = ServerUrl!
                }
            };
        }

        /// <inheritdoc/>
        public async Task RegisterVaultAsync(string vaultId, string? name, IKeyUsage dekKey, IKeyUsage macKey, CancellationToken cancellationToken = default)
        {
            if (_client is null)
                throw new InvalidOperationException("The App Platform connection has not been authenticated.");

            var user = await _client.GetMeAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(user.PublicKeyJwk))
                throw new InvalidOperationException("The user account is not set up.");

            var vaultKeyJwe = GetVaultJweKey(user, dekKey, macKey);
            await _client.RegisterVaultAsync(vaultId, name, vaultKeyJwe, description: null, cancellationToken);
        }

        private static unsafe string GetVaultJweKey(AppPlatformClient.UserInfo userInfo, IKeyUsage dekKey, IKeyUsage macKey)
        {
            return dekKey.UseKey(dek =>
            {
                fixed (byte* dekPtr = dek)
                {
                    var state = (dekPtr: (nint)dekPtr, dekLen: dek.Length);
                    return macKey.UseKey(state, (mac, s) =>
                    {
                        var localDek = new ReadOnlySpan<byte>((byte*)s.dekPtr, s.dekLen);
                        return JweHelper.EncryptVaultKey(localDek, mac, userInfo.PublicKeyJwk);
                    });
                }
            });
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _client?.Dispose();
            _client = null;
            base.Dispose();
        }
    }
}
