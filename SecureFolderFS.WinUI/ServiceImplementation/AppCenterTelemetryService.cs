using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Api;
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
        public Task EnableTelemetryAsync()
        {
            var appCenterKey = ApiKeys.GetAppCenterKey();
            if (!string.IsNullOrEmpty(appCenterKey) && !AppCenter.Configured)
                AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));

            return AppCenter.SetEnabledAsync(true);
        }

        /// <inheritdoc/>
        public Task DisableTelemetryAsync()
        {
            return AppCenter.SetEnabledAsync(false);
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName)
        {
            Analytics.TrackEvent(eventName);
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
            Analytics.TrackEvent($"Ex: {exception}\n{exception.StackTrace}");
        }
    }
}
