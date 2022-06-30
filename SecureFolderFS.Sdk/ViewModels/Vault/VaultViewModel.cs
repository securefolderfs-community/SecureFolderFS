using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Core.Instance;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    public sealed class VaultViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        /// <summary>
        /// Gets the vault health model used to report status on the vault.
        /// </summary>
        public IVaultHealthModel VaultHealthModel { get; }

        /// <summary>
        /// Gets the instance of the encrypting file system associated with this vault.
        /// </summary>
        [Obsolete("Use IEncryptedFileSystemService (maybe?)")]
        public IVaultInstance? VaultInstance { get; }

        public VaultViewModel(IVaultModel vaultModel, IVaultInstance vaultInstance, IVaultHealthModel vaultHealthModel)
        {
            VaultModel = vaultModel;
            VaultInstance = vaultInstance;
            VaultHealthModel = vaultHealthModel;
        }
    }
}
