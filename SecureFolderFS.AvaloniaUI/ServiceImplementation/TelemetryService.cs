using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO implement
    /// <inheritdoc cref="ITelemetryService"/>
    internal sealed class TelemetryService : ITelemetryService
    {
        /// <inheritdoc/>
        public Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task EnableTelemetryAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DisableTelemetryAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ReportTelemetry(string name, IDictionary<string, string>? properties = null)
        {
            throw new NotImplementedException();
        }
    }
}