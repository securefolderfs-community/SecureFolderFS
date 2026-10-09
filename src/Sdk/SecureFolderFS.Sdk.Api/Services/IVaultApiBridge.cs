using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.EventArguments;
using SecureFolderFS.Sdk.Api.Models;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// The single seam through which the API observes and affects live application state, keeping the
    /// protocol server free of any dependency on the app or its UI framework. Implementations marshal to
    /// the UI thread.
    /// </summary>
    public interface IVaultApiBridge
    {
        /// <summary>
        /// Gets whether the application has initialized to serve requests.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Occurs when a vault is added, removed, renamed, unlocked or locked.
        /// </summary>
        event EventHandler<ApiVaultChangedEventArgs>? VaultChanged;

        /// <summary>
        /// Gets every vault the application currently knows about.
        /// </summary>
        /// <returns>A collection of available vaults.</returns>
        IReadOnlyList<VaultInfo> GetVaults();

        /// <summary>
        /// Asks the application to show its own unlock prompt for a vault.
        /// </summary>
        /// <param name="vaultId">The identifier for the vault to unlock.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the outcome of the unlock attempt.</returns>
        Task<ApiActionOutcome> RequestUnlockAsync(string vaultId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locks an unlocked vault.
        /// </summary>
        /// <param name="vaultId">The identifier for the vault to lock.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the outcome of the lock attempt.</returns>
        Task<ApiActionOutcome> LockAsync(string vaultId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reveals the mounted root of an unlocked vault in the system file manager.
        /// </summary>
        /// <param name="vaultId">The identifier for the vault to reveal.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the outcome of the reveal attempt.</returns>
        Task<ApiActionOutcome> RevealAsync(string vaultId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Brings the main application window to the foreground.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the outcome of the bring-to-foreground attempt.</returns>
        Task<ApiActionOutcome> ShowMainWindowAsync(CancellationToken cancellationToken = default);
    }
}
