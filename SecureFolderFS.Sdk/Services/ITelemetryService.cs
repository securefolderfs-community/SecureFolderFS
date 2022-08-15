using System.Collections.Generic;
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
        /// Reports and sends the telemetry data.
        /// </summary>
        /// <param name="name">The name of the data.</param>
        /// <param name="properties">Optional properties representing the data.</param>
        void ReportTelemetry(string name, IDictionary<string, string>? properties = null);
    }
}
