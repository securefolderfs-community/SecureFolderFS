using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    [Bindable(true)]
    public sealed partial class UnlockedVaultViewModel : ObservableObject, IAsyncDisposable
    {
        [ObservableProperty] private bool _IsReadOnly;

        /// <summary>
        /// Gets the unlocked root folder of the vault.
        /// </summary>
        public IVFSRoot StorageRoot { get; }

        /// <summary>
        /// Gets the vault view model associated with the vault.
        /// </summary>
        public VaultViewModel VaultViewModel { get; }

        public UnlockedVaultViewModel(IVFSRoot storageRoot, VaultViewModel vaultViewModel)
        {
            StorageRoot = storageRoot;
            VaultViewModel = vaultViewModel;
            IsReadOnly = storageRoot.Options.IsReadOnly;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposableExtensions.TryDisposeAsync(StorageRoot);
        }
    }
}
