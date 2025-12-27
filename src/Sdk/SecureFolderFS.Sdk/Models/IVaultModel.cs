using System;
using System.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that represents a vault.
    /// </summary>
    public interface IVaultModel : INotifyStateChanged, IWrapper<IRemoteResource<IFolder>?>, IRemoteResource<IFolder>, IEquatable<IVaultModel>, IEquatable<VaultDataModel>, ISavePersistence
    {
        /// <summary>
        /// Gets a value indicating whether the vault is remotely stored.
        /// </summary>
        bool IsRemote { get; }

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
