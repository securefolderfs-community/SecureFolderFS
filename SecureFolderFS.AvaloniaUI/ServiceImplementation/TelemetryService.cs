using SecureFolderFS.Sdk.Services;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO: Implement telemetry
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
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DisableTelemetryAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName)
        {
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
        }
    }
}