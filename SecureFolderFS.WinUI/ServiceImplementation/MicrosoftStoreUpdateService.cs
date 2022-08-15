using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services;
using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.Enums;
using System.Threading.Tasks;
using Windows.Services.Store;
using System.Threading;

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
        public async Task<bool> InitializeAsync()
        {
            _storeContext ??= await Task.Run(StoreContext.GetDefault);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IsUpdateAvailableAsync()
        {
            AssertInitialized();

            try
            {
                _updates = await _storeContext!.GetAppAndOptionalStorePackageUpdatesAsync();
                return !_updates.IsEmpty();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<AppUpdateResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            AssertInitialized();

            var operation = _storeContext!.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
            operation.Progress = (asyncInfo, update) => progress?.Report(update.PackageDownloadProgress);
            var result = await operation.AsTask(cancellationToken);

            return result.OverallState switch
            {
                StorePackageUpdateState.Pending => AppUpdateResult.InProgress,
                StorePackageUpdateState.Downloading => AppUpdateResult.InProgress,
                StorePackageUpdateState.Deploying => AppUpdateResult.InProgress,
                StorePackageUpdateState.Completed => AppUpdateResult.Completed,
                StorePackageUpdateState.Canceled => AppUpdateResult.Canceled,
                StorePackageUpdateState.OtherError => AppUpdateResult.FailedUnknownError,
                StorePackageUpdateState.ErrorLowBattery => AppUpdateResult.FailedDeviceError,
                StorePackageUpdateState.ErrorWiFiRecommended => AppUpdateResult.FailedNetworkError,
                StorePackageUpdateState.ErrorWiFiRequired => AppUpdateResult.FailedNetworkError,
                _ => AppUpdateResult.FailedUnknownError
            };
        }

        private void AssertInitialized()
        {
            _ = _storeContext ?? throw new InvalidOperationException($"{nameof(_storeContext)} was not initialized.");
        }
    }
}
