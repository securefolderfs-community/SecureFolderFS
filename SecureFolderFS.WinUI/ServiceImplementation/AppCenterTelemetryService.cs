using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class AppCenterTelemetryService : ITelemetryService
    {
        public async Task EnableTelemetry()
        {
            if (!AppCenter.Configured)
            {
                AppCenter.Start();
            }
            else
            {
                await AppCenter.SetEnabledAsync(true);
            }
        }

        public async Task DisableTelemetry()
        {
            await AppCenter.SetEnabledAsync(false);
        }

        public void ReportTelemetry(string name, IDictionary<string, string>? properties = null)
        {
            Analytics.TrackEvent(name, properties);
        }
    }
}
