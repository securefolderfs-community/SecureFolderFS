using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>]
    public sealed partial class VaultListItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _CanRemoveVault = true;
        [ObservableProperty] private DateTime? _LastAccessDate;

        public VaultViewModel VaultViewModel { get; }

        public VaultListItemViewModel(VaultViewModel vaultViewModel, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            VaultViewModel = vaultViewModel;
            _vaultCollectionModel = vaultCollectionModel;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
            {
                // Prevent from removing vault if it is unlocked
                CanRemoveVault = false;

                // Update last accessed date
                LastAccessDate = VaultViewModel.VaultModel.LastAccessDate;
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            _vaultCollectionModel.Remove(VaultViewModel.VaultModel);
            return _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.TryOpenInFileExplorerAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
        }
    }
}
