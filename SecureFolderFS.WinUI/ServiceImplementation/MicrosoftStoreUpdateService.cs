using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IUpdateService"/>
    internal sealed class MicrosoftStoreUpdateService : IUpdateService
    {
        private StoreContext? _storeContext;
        private IEnumerable<StorePackageUpdate>? _updates;

        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<bool> IsUpdateAvailableAsync(CancellationToken cancellationToken = default)
        {
            if (!TrySetStoreContext() || _storeContext is null)
                return false;

            try
            {
                _updates = await _storeContext.GetAppAndOptionalStorePackageUpdatesAsync().AsTask(cancellationToken);
                return !_updates.IsEmpty();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<AppUpdateResultType> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            if (!TrySetStoreContext() || _storeContext is null)
                return AppUpdateResultType.FailedUnknownError;

            var operation = _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
            operation.Progress = (_, update) => progress?.Report(update.PackageDownloadProgress);
            var result = await operation.AsTask(cancellationToken);

            return result.OverallState switch
            {
                StorePackageUpdateState.Pending => AppUpdateResultType.InProgress,
                StorePackageUpdateState.Downloading => AppUpdateResultType.InProgress,
                StorePackageUpdateState.Deploying => AppUpdateResultType.InProgress,
                StorePackageUpdateState.Completed => AppUpdateResultType.Completed,
                StorePackageUpdateState.Canceled => AppUpdateResultType.Canceled,
                StorePackageUpdateState.OtherError => AppUpdateResultType.FailedUnknownError,
                StorePackageUpdateState.ErrorLowBattery => AppUpdateResultType.FailedDeviceError,
                StorePackageUpdateState.ErrorWiFiRecommended => AppUpdateResultType.FailedNetworkError,
                StorePackageUpdateState.ErrorWiFiRequired => AppUpdateResultType.FailedNetworkError,
                _ => AppUpdateResultType.FailedUnknownError
            };
        }

        private bool TrySetStoreContext()
        {
            _storeContext ??= StoreContext.GetDefault();
            return _storeContext is not null;
        }
    }
}
