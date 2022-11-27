using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that represents a vault.
    /// </summary>
    public interface IVaultModel : IEquatable<IVaultModel>
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
        /// Gets the date of last vault access time.
        /// </summary>
        DateTime LastAccessedDate { get; }

        /// <summary>
        /// Tries to access the vault, updating <see cref="LastAccessedDate"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task AccessVaultAsync(CancellationToken cancellationToken = default);
    }
}
