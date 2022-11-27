using SecureFolderFS.Sdk.Storage;
using System;

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
    }
}
