using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Manages the local integration API on behalf of the settings interface.
    /// </summary>
    public interface ILocalIntegrationsService
    {
        /// <summary>
        /// Starts or stops the API endpoint and records the choice.
        /// </summary>
        /// <param name="isEnabled">Whether other applications may connect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets every application currently authorized to connect.
        /// </summary>
        IReadOnlyList<IntegrationClientInfo> GetClients();

        /// <summary>
        /// Withdraws an application's authorization. It must ask the user again to reconnect.
        /// </summary>
        /// <param name="clientId">The identifier from <see cref="IntegrationClientInfo.Id"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RevokeClientAsync(string clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Withdraws every application's authorization and forgets every recorded refusal, so a mistaken
        /// denial does not leave the application waiting out the cooldown.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RevokeAllClientsAsync(CancellationToken cancellationToken = default);
    }
}
