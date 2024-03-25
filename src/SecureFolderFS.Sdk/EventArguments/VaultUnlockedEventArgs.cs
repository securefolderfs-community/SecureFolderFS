using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault unlocked events.
    /// </summary>
    public sealed class VaultUnlockedEventArgs(IFolder storageRoot, IVaultModel vaultModel) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IFolder"/> that represents the storage root of the unlocked vault.
        /// </summary>
        public IFolder StorageRoot { get; } = storageRoot;

        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the unlocked vault.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
