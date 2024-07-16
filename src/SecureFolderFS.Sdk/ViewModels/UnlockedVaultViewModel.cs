using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    public sealed class UnlockedVaultViewModel : ObservableObject, IAsyncDisposable
    {
        /// <summary>
        /// Gets the unlocked root folder of the vault.
        /// </summary>
        public IVFSRoot StorageRoot { get; }

        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public UnlockedVaultViewModel(IVFSRoot storageRoot, IVaultModel vaultModel)
        {
            StorageRoot = storageRoot;
            VaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposableExtensions.TryDisposeAsync(StorageRoot);
        }
    }
}
