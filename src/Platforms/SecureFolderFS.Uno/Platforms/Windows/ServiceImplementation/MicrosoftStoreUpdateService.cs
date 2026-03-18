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
using Windows.Foundation;
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
        public async Task<IResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            if (!await SetStoreContextAsync() || _storeContext is null)
                return Result<AppUpdateResultType>.Failure(AppUpdateResultType.FailedUnknownError);

            try
            {
                if (_updates is null && !await IsUpdateAvailableAsync(cancellationToken))
                    return Result.Failure(new InvalidOperationException("No available updates found."));

                // RequestDownloadAndInstallStorePackageUpdatesAsync requires the UI thread (HWND context).
                // We use PostOrExecuteAsync to marshal onto the captured UI SynchronizationContext.
                var uiContext = ThreadingService.GetContext();
                IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus>? operation = null;

                await uiContext.PostOrExecuteAsync(() =>
                {
                    operation = _storeContext.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);
                    return Task.CompletedTask;
                });

                if (operation is null)
                    return Result<AppUpdateResultType>.Failure(AppUpdateResultType.FailedUnknownError);

                // Add progress operation callback
                operation.Progress = (_, update) =>
                {
                    uiContext.PostOrExecute(_ => StateChanged?.Invoke(this, new UpdateChangedEventArgs(GetAppUpdateResultType(update.PackageUpdateState))));

                    // According to docs, PackageDownloadProgress ranges from 0.0 to 0.8 (inclusive)
                    // where 0.8 marks the end of the download stage - re-adjust to 0-100% range.
                    progress?.Report(update.PackageDownloadProgress * 100d / 0.8d);
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

        private async Task<bool> SetStoreContextAsync()
        {
            if (_storeContext is null)
            {
                var uiContext = ThreadingService.GetContext();
                await uiContext.PostOrExecuteAsync(() =>
                {
                    _storeContext = StoreContext.GetDefault();
                    if (_storeContext is not null)
                        WinRT_InitializeObject(_storeContext);

                    return Task.CompletedTask;
                });
            }

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

        private static void WinRT_InitializeObject(object obj)
        {
            _ = obj;
#if WINDOWS
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Instance?.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
#endif
        }
    }
}
