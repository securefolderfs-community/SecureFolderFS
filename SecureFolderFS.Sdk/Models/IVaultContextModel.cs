using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a context that holds saved data of an individual vault.
    /// </summary>
    public interface IVaultContextModel
    {
        /// <summary>
        /// Gets the vault model that is associated with this context.
        /// </summary>
        IVaultModel VaultModel { get; }

        /// <summary>
        /// Gets the last accessed date of the <see cref="VaultModel"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="DateTime"/> representing the last access date, otherwise null.</returns>
        Task<DateTime?> GetLastAccessedDate(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the last accessed date for the <see cref="VaultModel"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and the value was set, returns true, otherwise false.</returns>
        Task<bool> SetLastAccessedDate(DateTime? value, CancellationToken cancellationToken = default);
    }
}
