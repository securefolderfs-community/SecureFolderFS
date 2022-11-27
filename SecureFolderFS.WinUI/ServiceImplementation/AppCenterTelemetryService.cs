using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
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
        public void ReportTelemetry(string name, IDictionary<string, string>? properties = null)
        {
            Analytics.TrackEvent(name, properties);
        }
    }
}
