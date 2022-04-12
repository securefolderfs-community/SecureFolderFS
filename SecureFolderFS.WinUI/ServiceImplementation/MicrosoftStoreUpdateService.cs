using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Services;
using System;
using System.Collections.Generic;
using SecureFolderFS.Backend.Enums;
using System.Threading.Tasks;
using Windows.Services.Store;
using System.Diagnostics;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class MicrosoftStoreUpdateService : IUpdateService
    {
        private StoreContext? _storeContext;

        private IEnumerable<StorePackageUpdate>? _updates;

        public async Task<bool> AreAppUpdatesSupportedAsync()
        {
            _storeContext ??= await Task.Run(StoreContext.GetDefault);

            return false;
        }

        public async Task<bool> IsNewUpdateAvailableAsync()
        {
            _ = _storeContext ?? throw new InvalidOperationException($"{nameof(_storeContext)} was not initialized.");

            try
            {
                _updates = await _storeContext.GetAppAndOptionalStorePackageUpdatesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return !_updates.IsEmpty();
        }

        public async Task<AppUpdateResult> UpdateAppAsync(IProgress<double>? progress)
        {
            _ = _storeContext ?? throw new InvalidOperationException($"{nameof(_storeContext)} was not initialized.");
            _ = _updates ?? throw new InvalidOperationException($"{nameof(_updates)} was not initialized.");

            var operation = _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
            operation.Progress = (asyncInfo, update) =>
            {
                progress?.Report(update.PackageDownloadProgress);
            };

            var result = await operation;

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
    }
}
