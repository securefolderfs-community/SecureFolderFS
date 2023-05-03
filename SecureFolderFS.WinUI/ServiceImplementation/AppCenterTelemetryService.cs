using Microsoft.AppCenter;
using SecureFolderFS.Sdk.Services;
using System;
using System.Threading.Tasks;

#if !DEBUG
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.UI.Api;
#endif

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
#if !DEBUG
            var appCenterKey = ApiKeys.GetAppCenterKey();
            if (!string.IsNullOrEmpty(appCenterKey) && !AppCenter.Configured)
                AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));

            return AppCenter.SetEnabledAsync(true);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc/>
        public Task DisableTelemetryAsync()
        {
#if !DEBUG
            return AppCenter.SetEnabledAsync(false);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName)
        {
#if !DEBUG
            Analytics.TrackEvent(eventName);
#endif
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
#if !DEBUG
            Analytics.TrackEvent($"Ex: {exception}\n{exception.StackTrace}");
#endif
        }
    }
}
