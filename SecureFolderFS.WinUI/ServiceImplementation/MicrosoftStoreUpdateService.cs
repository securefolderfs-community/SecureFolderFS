using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
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

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<bool> IsUpdateAvailableAsync(CancellationToken cancellationToken = default)
        {
            if (!await SetStoreContextAsync() || _storeContext is null)
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
        public async Task<IResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            if (!await SetStoreContextAsync() || _storeContext is null)
                return new CommonResult<AppUpdateResultType>(AppUpdateResultType.FailedUnknownError, false);

            try
            {
                await ThreadingService.ChangeThreadAsync();

                var operation = _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
                operation.Progress = (_, update) => progress?.Report(update.PackageDownloadProgress);
                var result = await operation.AsTask(cancellationToken);

                var resultType = result.OverallState switch
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

                return new CommonResult<AppUpdateResultType>(resultType, resultType == AppUpdateResultType.Completed);
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        private async Task<bool> SetStoreContextAsync()
        {
            _storeContext ??= await Task.Run(StoreContext.GetDefault);
            return _storeContext is not null;
        }
    }
}
