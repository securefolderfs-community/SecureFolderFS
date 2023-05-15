using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class MicrosoftStoreIapService : IIapService
    {
        /// <inheritdoc/>
        public Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("");
        }
    }
}
