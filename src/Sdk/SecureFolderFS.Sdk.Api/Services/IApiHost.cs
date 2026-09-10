using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// Hosts the local integration API endpoint.
    /// </summary>
    public interface IApiHost : IAsyncDisposable
    {
        /// <summary>
        /// Binds the endpoint and begins accepting clients.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops accepting clients and disconnects existing ones.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <remarks>
        /// The endpoint file is left advertising the API as enabled, so a client can tell "turned off" from "not running".
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Records in the endpoint file that the user has disabled local integrations.
        /// </summary>
        void PublishDisabled();
    }
}
