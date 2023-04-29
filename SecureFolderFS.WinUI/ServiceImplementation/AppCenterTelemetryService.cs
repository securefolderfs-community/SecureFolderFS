using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.Sdk.Services;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ITelemetryService"/>
    internal sealed class AppCenterTelemetryService : ITelemetryService
    {
        /// <inheritdoc/>
        public async Task<bool> IsEnabledAsync()
        {
            return await AppCenter.IsEnabledAsync();
        }

        /// <inheritdoc/>
        public async Task EnableTelemetryAsync()
        {
            if (!AppCenter.Configured)
                AppCenter.Start(typeof(Analytics), typeof(Crashes));
                
            await AppCenter.SetEnabledAsync(true);
        }

        /// <inheritdoc/>
        public async Task DisableTelemetryAsync()
        {
            await AppCenter.SetEnabledAsync(false);
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName)
        {
            Analytics.TrackEvent(eventName);
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
            Analytics.TrackEvent($"Exception: {exception}");
        }
    }
}
