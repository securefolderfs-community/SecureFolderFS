using SecureFolderFS.Sdk.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: needs docs
    public interface IIapService
    {
        Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default);

        Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default);

        Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default);
    }
}
