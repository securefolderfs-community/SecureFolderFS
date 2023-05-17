using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IIapService"/>
    public sealed class DebugIapService : IIapService
    {
        /// <inheritdoc/>
        public Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
