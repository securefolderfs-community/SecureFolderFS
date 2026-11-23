#if APP_PLATFORM_PRESENT
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppPlatform.Services;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <summary>
    /// <see cref="IAccountProvider"/> for App Platform device-key identities, backed by <see cref="IDeviceKeyStore"/>.
    /// </summary>
    public sealed class AppPlatformAccountProvider : IAccountProvider
    {
        private readonly IDeviceKeyStore _deviceKeyStore;

        /// <inheritdoc/>
        public string ProviderId { get; } = Core.Constants.Vault.Authentication.AUTH_APP_PLATFORM;

        public AppPlatformAccountProvider(IDeviceKeyStore deviceKeyStore)
        {
            _deviceKeyStore = deviceKeyStore;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<AccountModel>> GetAccountsAsync(CancellationToken cancellationToken = default)
        {
            var mediaService = DI.Service<IMediaService>();
            var icon = await mediaService.GetImageFromResourceAsync("AppPlatformIcon", cancellationToken);
            
            var accounts = await _deviceKeyStore.GetAccountsAsync(cancellationToken);
            return accounts
                .Select(x => new AccountModel(
                    x.Id,
                    x.DisplayName,
                    x.ServerUrl,
                    icon,
                    ProviderId))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task RemoveAccountAsync(string accountId, CancellationToken cancellationToken = default)
        {
            return _deviceKeyStore.ClearAsync(accountId, cancellationToken);
        }
    }
}
#endif
