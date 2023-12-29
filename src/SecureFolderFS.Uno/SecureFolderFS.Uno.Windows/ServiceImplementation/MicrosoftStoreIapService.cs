using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using Windows.Services.Store;

namespace SecureFolderFS.Uno.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IIapService"/>
    internal sealed class MicrosoftStoreIapService : IIapService
    {
        private StoreContext? _storeContext;

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            if (!await SetStoreContextAsync() || _storeContext is null)
                return false;

            var iapId = GetIapId(productType);
            if (iapId is null)
                return false;

            var appLicense = await _storeContext.GetAppLicenseAsync().AsTask(cancellationToken);
            if (appLicense is null)
                return false;

            foreach (var item in appLicense.AddOnLicenses)
            {
                var license = item.Value;
                if (!license.IsActive)
                    continue;

                if (license.InAppOfferToken.Contains(iapId))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> PurchaseAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            var iapId = GetIapId(productType);
            if (iapId is null)
                return false;

            var product = await GetProductAsync(iapId, cancellationToken);
            if (product is null)
                return false;

            var purchaseResult = await product.RequestPurchaseAsync();
            if (purchaseResult is null)
                return false;

            return purchaseResult.ExtendedError is null;
        }

        /// <inheritdoc/>
        public async Task<string?> GetPriceAsync(IapProductType productType, CancellationToken cancellationToken = default)
        {
            var iapId = GetIapId(productType);
            if (iapId is null)
                return null;

            var product = await GetProductAsync(iapId, cancellationToken);
            if (product is null)
                return null;

            return product.Price.FormattedPrice;
        }

        private async Task<StoreProduct?> GetProductAsync(string iapId, CancellationToken cancellationToken)
        {
            if (!await SetStoreContextAsync() || _storeContext is null)
                return null;

            var result = await _storeContext.GetAssociatedStoreProductsAsync(new[] { "Durable" }).AsTask(cancellationToken);
            if (result.ExtendedError is not null)
                return null;

            foreach (var item in result.Products.Values)
            {
                if (item.InAppOfferToken.Contains(iapId))
                    return item;
            }

            return null;
        }

        private string? GetIapId(IapProductType productType)
        {
            return productType switch
            {
                IapProductType.SecureFolderFSPlus => Constants.IAP_SECUREFOLDERFS_PLUS_ID,
                _ => null
            };
        }

        private async Task<bool> SetStoreContextAsync()
        {
            _storeContext ??= await Task.Run(StoreContext.GetDefault);
            return _storeContext is not null;
        }
    }
}
