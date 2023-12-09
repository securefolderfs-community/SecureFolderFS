using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that represents a vault.
    /// </summary>
    public interface IVaultModel
    {
        /// <summary>
        /// Gets the folder of the vault.
        /// </summary>
        IFolder Folder { get; }

        /// <summary>
        /// Gets the name of the vault.
        /// </summary>
        string VaultName { get; }

        /// <summary>
        /// Gets the last accessed date for this vault.
        /// </summary>
        DateTime? LastAccessDate { get; }

        /// <summary>
        /// Sets the <see cref="LastAccessDate"/> for this vault.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the value was successfully set, returns true; otherwise false.</returns>
        Task<bool> SetLastAccessDateAsync(DateTime? value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="VaultName"/> for this vault.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the value was successfully set, returns true; otherwise false.</returns>
        Task<bool> SetVaultNameAsync(string value, CancellationToken cancellationToken = default);
    }
}
