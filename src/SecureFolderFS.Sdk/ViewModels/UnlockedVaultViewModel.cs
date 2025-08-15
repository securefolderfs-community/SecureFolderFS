using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    [Bindable(true)]
    public sealed partial class UnlockedVaultViewModel : ObservableObject, IAsyncDisposable
    {
        /// <summary>
        /// Gets the folder associated with this vault.
        /// </summary>
        public IFolder VaultFolder { get; }

        /// <summary>
        /// Gets the unlocked root folder of the vault.
        /// </summary>
        public IVFSRoot StorageRoot { get; }

        /// <summary>
        /// Gets the options of the unlocked vault.
        /// </summary>
        public VirtualFileSystemOptions Options { get; }

        /// <summary>
        /// Gets the vault view model associated with the vault.
        /// </summary>
        public VaultViewModel VaultViewModel { get; }

        public UnlockedVaultViewModel(IFolder vaultFolder, IVFSRoot storageRoot, VaultViewModel vaultViewModel)
        {
            VaultFolder = vaultFolder;
            StorageRoot = storageRoot;
            VaultViewModel = vaultViewModel;
            Options = storageRoot.Options;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposableExtensions.TryDisposeAsync(StorageRoot);
        }
    }
}
