using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

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
        /// Gets the lock that restricts the deletion of the folder.
        /// </summary>
        IDisposable? FolderLock { get; }

        /// <summary>
        /// Determines if <see cref="Folder"/> is still valid.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and the folder is accessible, returns true, otherwise false.</returns>
        Task<bool> IsAccessibleAsync();
    }
}
