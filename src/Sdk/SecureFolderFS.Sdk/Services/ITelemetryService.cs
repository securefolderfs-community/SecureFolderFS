using SecureFolderFS.Sdk.Enums;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage app telemetry.
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Checks if the telemetry is enabled.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If enabled, returns true otherwise false.</returns>
        Task<bool> IsEnabledAsync();

        /// <summary>
        /// Enables optional telemetry and usage data.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task EnableTelemetryAsync();

        /// <summary>
        /// Disables optional telemetry and usage data.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task DisableTelemetryAsync();

        /// <summary>
        /// Reports and sends specified <paramref name="message"/> to the telemetry service.
        /// </summary>
        /// <param name="message">The name of the event that occurred within the app.</param>
        /// <param name="severity">The severity of the message to log.</param>
        void TrackMessage(string message, Severity severity);

        /// <summary>
        /// Reports and sends specified <paramref name="exception"/> to the telemetry service.
        /// </summary>
        /// <param name="exception">The error that occurred during code execution.</param>
        void TrackException(Exception exception);
    }
}
