using SecureFolderFS.Sdk.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage in-app purchases.
    /// </summary>
    public interface IIapService
    {
        /// <summary>
        /// Determines whether the specified in-app purchase product is owned by the user.
        /// </summary>
        /// <param name="productType">The type of the in-app purchase product to check for ownership.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true, if the product is owned; otherwise, false.</returns>
        Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiates the purchase process for the specified in-app purchase product.
        /// </summary>
        /// <param name="productType">The type of the in-app purchase product to be purchased.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if the purchase was successful; otherwise, false.</returns>
        Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the price of the specified in-app purchase product.
        /// </summary>
        /// <param name="productType">The type of the in-app purchase product for which the price is to be retrieved.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the price of the product as a string, or null if the price cannot be determined.</returns>
        Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default);
    }
}
