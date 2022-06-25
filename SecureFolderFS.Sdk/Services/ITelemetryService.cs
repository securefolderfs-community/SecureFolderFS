using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface ITelemetryService
    {
        Task EnableTelemetry();

        Task DisableTelemetry();

        void ReportTelemetry(string name, IDictionary<string, string>? properties = null);
    }
}
