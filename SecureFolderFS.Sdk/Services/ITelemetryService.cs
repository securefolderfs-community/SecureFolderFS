using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface ITelemetryService
    {
        bool IsTelemetryEnabled { get; }

        Task EnableTelemetry();

        Task DisableTelemetry();

        void ReportTelemetry(string name, IDictionary<string, string>? properties = null);
    }
}
