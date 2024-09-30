using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using Windows.ApplicationModel;
using Windows.Services.Store;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IUpdateService"/>
    internal sealed class MicrosoftStoreUpdateService : IUpdateService
    {
        private StoreContext? _storeContext;
        private IEnumerable<StorePackageUpdate>? _updates;

        private IThreadingService ThreadingService { get; } = DI.Service<IThreadingService>();

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            var supported = Package.Current.SignatureKind == PackageSignatureKind.Store;
            return Task.FromResult(supported);
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
                return Result<AppUpdateResultType>.Failure(AppUpdateResultType.FailedUnknownError);

            try
            {
                if (_updates is null && !await IsUpdateAvailableAsync(cancellationToken))
                    return Result.Failure(new InvalidOperationException("No available updates found."));

                // Switch to UI thread for installation of packages (as per docs)
                await ThreadingService.ChangeThreadAsync();

                // Add progress operation callback
                var operation = _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
                operation.Progress = (_, update) =>
                {
                    StateChanged?.Invoke(this, new UpdateChangedEventArgs(GetAppUpdateResultType(update.PackageUpdateState)));

                    // According to docs, the PackageDownloadProgress ranges from 0.0 to 0.8 (inclusive)
                    // which indicates the download progress where 0.8 is the end of the download stage.
                    // Therefore, we need to re-adjust the value to fit the percentage range 0-100%
                    var percentage = update.PackageDownloadProgress * 100 / 0.8d;

                    // Report the percentage without rounding
                    progress?.Report(percentage);
                };

                // Install packages
                var result = await operation.AsTask(cancellationToken);
                var resultType = GetAppUpdateResultType(result.OverallState);

                return resultType == AppUpdateResultType.Completed
                        ? Result<AppUpdateResultType>.Success(resultType)
                        : Result<AppUpdateResultType>.Failure(resultType);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        private async Task<bool> SetStoreContextAsync()
        {
            _storeContext ??= await Task.Run(StoreContext.GetDefault);
            return _storeContext is not null;
        }

        private static AppUpdateResultType GetAppUpdateResultType(StorePackageUpdateState updateState)
        {
            return updateState switch
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
                _ => AppUpdateResultType.None
            };
        }
    }
}
