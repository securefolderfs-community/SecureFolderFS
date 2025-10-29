using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ITelemetryService"/>
    public sealed class DebugTelemetryService : ITelemetryService
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
        public void TrackMessage(string eventName, Severity severity)
        {
            _ = eventName;
            _ = severity;
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
            _ = exception;
        }
    }
}
