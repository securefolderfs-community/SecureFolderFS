using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultListItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _CanMove;
        [ObservableProperty] private bool _CanMoveUp;
        [ObservableProperty] private bool _CanMoveDown;
        [ObservableProperty] private bool _CanRemoveVault;
        [ObservableProperty] private VaultViewModel _VaultViewModel;

        public VaultListItemViewModel(VaultViewModel vaultViewModel, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            CanRemoveVault = true;
            VaultViewModel = vaultViewModel;
            _vaultCollectionModel = vaultCollectionModel;

            UpdateCanMove();
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
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private async Task MoveItemAsync(string? direction, CancellationToken cancellationToken)
        {
            var oldIndex = _vaultCollectionModel.IndexOf(VaultViewModel.VaultModel);
            var newIndex = direction?.ToLower() switch
            {
                "up" => Math.Max(oldIndex - 1, 0),
                "down" => Math.Min(oldIndex + 1, _vaultCollectionModel.Count - 1),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };

            _vaultCollectionModel.Move(oldIndex, newIndex);
            UpdateCanMove();

            // Save after move
            await _vaultCollectionModel.SaveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            _vaultCollectionModel.Remove(VaultViewModel.VaultModel);
            await _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.TryOpenInFileExplorerAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
        }

        private void UpdateCanMove()
        {
            var itemIndex = _vaultCollectionModel.IndexOf(VaultViewModel.VaultModel);
            CanMoveDown = itemIndex < _vaultCollectionModel.Count - 1;
            CanMoveUp = itemIndex > 0;
            CanMove = CanMoveUp && CanMoveDown;
        }
    }
}
