using System;
using System.Collections.Generic;

namespace SecureFolderFS.Backend.Services
{
    public interface ITelemetryService
    {
        bool IsTelemetryEnabled { get; }

        Task EnableTelemetry();

        Task DisableTelemetry();

        void ReportTelemetry(string name, IDictionary<string, string>? properties = null);
    }
}
