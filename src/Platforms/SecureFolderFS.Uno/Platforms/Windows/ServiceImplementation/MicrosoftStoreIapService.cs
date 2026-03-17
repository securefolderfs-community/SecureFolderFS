using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using Windows.Services.Store;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IIapService"/>
    internal sealed class MicrosoftStoreIapService : IIapService
    {
        private const string IAP_SECUREFOLDERFS_PLUS_LIFETIME_TOKEN = "plus_lifetime";
        private const string IAP_SECUREFOLDERFS_PLUS_SUBSCRIPTION_TOKEN = "plus_subscription";

        private readonly Lazy<StoreContext> _storeContext = new(StoreContext.GetDefault);

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            // For Any, check all individual types and return true if at least one is owned.
            if (productType == IapProductType.Any)
            {
                // Enumerate all concrete (non-composite) types by excluding Any itself.
                foreach (var type in Enum.GetValues<IapProductType>().Where(t => t != IapProductType.Any))
                {
                    if (await IsOwnedAsync(type, cancellationToken))
                        return true;
                }

                return false;
            }

            var token = GetIapToken(productType);
            if (token is null)
                return false;

            StoreAppLicense? appLicense;
            try
            {
                appLicense = await _storeContext.Value.GetAppLicenseAsync().AsTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            if (appLicense is null)
                return false;

            foreach (var item in appLicense.AddOnLicenses)
            {
                var license = item.Value;
                if (license.IsActive && license.InAppOfferToken == token)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            if (productType == IapProductType.Any)
                return false; // Ambiguous - caller must specify a concrete product type

            var token = GetIapToken(productType);
            if (token is null)
                return false;

            var product = await GetProductAsync(productType, token, cancellationToken);
            if (product is null)
                return false;

            StorePurchaseResult? purchaseResult;
            try
            {
                purchaseResult = await product.RequestPurchaseAsync().AsTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            if (purchaseResult is null)
                return false;

            return purchaseResult.Status is StorePurchaseStatus.Succeeded
                or StorePurchaseStatus.AlreadyPurchased;
        }

        /// <inheritdoc/>
        public async Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            if (productType == IapProductType.Any)
                return null; // Ambiguous - caller must specify a concrete product type

            var token = GetIapToken(productType);
            if (token is null)
                return null;

            var product = await GetProductAsync(productType, token, cancellationToken);
            return product?.Price.FormattedPrice;
        }

        private async Task<StoreProduct?> GetProductAsync(IapProductType productType, string token, CancellationToken cancellationToken)
        {
            // Durables cover lifetime purchases; Subscriptions need their own kind.
            // Query both so this method works regardless of product type passed.
            string[] productKinds = productType == IapProductType.PlusSubscription
                ? ["Subscription"]
                : ["Durable"];

            StoreProductQueryResult result;
            try
            {
                result = await _storeContext.Value
                    .GetAssociatedStoreProductsAsync(productKinds)
                    .AsTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            if (result.ExtendedError is not null)
                return null;

            foreach (var item in result.Products.Values)
            {
                if (item.InAppOfferToken == token)
                    return item;
            }

            return null;
        }

        private static string? GetIapToken(IapProductType productType)
        {
            return productType switch
            {
                IapProductType.PlusLifetime => IAP_SECUREFOLDERFS_PLUS_LIFETIME_TOKEN,
                IapProductType.PlusSubscription => IAP_SECUREFOLDERFS_PLUS_SUBSCRIPTION_TOKEN,
                _ => null
            };
        }
    }
}
