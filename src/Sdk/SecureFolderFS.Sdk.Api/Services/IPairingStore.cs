using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// Persists the applications the user has authorized and the tokens that identify them.
    /// </summary>
    /// <remarks>
    /// Tokens are stored only as SHA-256 digests. An individual token carries 256 bits of generator entropy,
    /// so a plain digest is the right construction.
    /// </remarks>
    public interface IPairingStore : IPersistable
    {
        /// <summary>
        /// Gets the per-installation vault ID key used to derive public vault identifiers.
        /// </summary>
        byte[] VaultIdKey { get; }

        /// <summary>
        /// Gets how long an application must wait before prompting again after being refused.
        /// </summary>
        TimeSpan DenialCooldown { get; }

        /// <summary>
        /// Gets every currently paired application.
        /// </summary>
        IReadOnlyList<PairedClient> GetClients();

        /// <summary>
        /// Finds the pairing a token belongs to, or <see langword="null"/> when unrecognized.
        /// </summary>
        PairedClient? Authenticate(string? token);

        /// <summary>
        /// Determines whether an application is still within its post-refusal cooldown.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to check against.</param>
        /// <returns>True if is in cooldown; otherwise, false.</returns>
        bool IsInDenialCooldown(string fingerprint);

        /// <summary>
        /// Records that a pairing was used, for display in the paired-clients list.
        /// </summary>
        /// <param name="clientId">The identifier of the pairing.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task TouchLastUsedAsync(string clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a pairing and issues its token, which is never stored in recoverable form.
        /// </summary>
        /// <param name="displayName">The display name of the application.</param>
        /// <param name="evidence">The peer evidence of the application.</param>
        /// <param name="scopes">The scopes the application is requesting.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the issued token.</returns>
        Task<string> CreateClientAsync(string displayName, PeerEvidence evidence, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a pairing, so the application must ask for consent again.
        /// </summary>
        /// <param name="clientId">The identifier of the pairing.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RevokeClientAsync(string clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes every pairing and forgets every recorded refusal.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RevokeAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Records that the user refused an application.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the application.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RecordDenialAsync(string fingerprint, CancellationToken cancellationToken = default);
    }
}
