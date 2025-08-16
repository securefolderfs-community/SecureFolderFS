using System;
using OwlCore.Storage;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that represents a vault.
    /// </summary>
    public interface IVaultModel : IRemoteResource<IFolder>, IEquatable<IVaultModel>, IEquatable<VaultDataModel>, ISavePersistence
    {
        /// <summary>
        /// Gets the folder of the vault.
        /// </summary>
        IFolder? VaultFolder { get; }

        /// <summary>
        /// Gets the data model associated with the vault.
        /// </summary>
        VaultDataModel DataModel { get; }
    }
}
