using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IUpdateService"/>
    public sealed class DebugUpdateService : IUpdateService
    {
        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> IsUpdateAvailableAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000);
            return true;
        }

        /// <inheritdoc/>
        public async Task<IResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            var progressValue = 0.0d;
            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(750);

                var state = i switch
                {
                    0 => AppUpdateResultType.InProgress,
                    1 => AppUpdateResultType.FailedDeviceError,
                    2 => AppUpdateResultType.FailedNetworkError,
                    3 => AppUpdateResultType.FailedUnknownError,
                    4 => AppUpdateResultType.Completed
                };
                StateChanged?.Invoke(this, new UpdateChangedEventArgs(state));

                progress?.Report(progressValue);
                progressValue += 25d;
            }

            return CommonResult.Success;
        }
    }
}
