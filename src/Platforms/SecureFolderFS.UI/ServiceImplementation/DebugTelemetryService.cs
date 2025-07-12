using SecureFolderFS.Sdk.Services;
using System;
using System.Threading.Tasks;

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
        public void TrackMessage(string eventName)
        {
            _ = eventName;
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
            _ = exception;
        }
    }
}
