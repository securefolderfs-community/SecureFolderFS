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
using OwlCore.Storage;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>, Inject<IMediaService>]
    [Bindable(true)]
    public sealed partial class VaultListItemViewModel : ObservableObject, IAsyncInitialize, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _IsRenaming;
        [ObservableProperty] private bool _IsUnlocked;
        [ObservableProperty] private bool _CanMoveDown;
        [ObservableProperty] private bool _CanMoveUp;
        [ObservableProperty] private bool _CanMove;
        [ObservableProperty] private IImage? _CustomIcon;
        [ObservableProperty] private VaultViewModel _VaultViewModel;

        public VaultListItemViewModel(VaultViewModel vaultViewModel, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            IsUnlocked = false;
            VaultViewModel = vaultViewModel;
            _vaultCollectionModel = vaultCollectionModel;

            UpdateCanMove();
            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await UpdateIconAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
            {
                // Prevent from removing vault if it is unlocked
                IsUnlocked = true;
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                IsUnlocked = false;
        }

        [RelayCommand]
        private void RequestLock()
        {
            WeakReferenceMessenger.Default.Send(new VaultLockRequestedMessage(VaultViewModel.VaultModel));
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
        private async Task CustomizeAsync(string? option, CancellationToken cancellationToken)
        {
            switch (option?.ToLower())
            {
                case "icon":
                {
                    if (VaultViewModel.VaultModel.Folder is not IModifiableFolder modifiableFolder)
                        return;

                    var sourceIconFile = await FileExplorerService.PickFileAsync(null, false, cancellationToken);
                    if (sourceIconFile is null)
                        return;

                    // Update vault icon
                    var destinationIconFile = await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_ICON_FILENAME, true, cancellationToken);
                    await sourceIconFile.CopyContentsToAsync(destinationIconFile, cancellationToken); // TODO: Resize icon (don't load large icons)
                    await UpdateIconAsync(cancellationToken);

                    // Update folder icon
                    await using var iconStream = await sourceIconFile.OpenReadAsync(cancellationToken);
                    await MediaService.TrySetFolderIconAsync(modifiableFolder, iconStream, cancellationToken);

                    break;
                }

                case "name": // TODO: Use this on mobile platforms where having an overlay is desirable
                {
                    var overlayViewModel = new RenameOverlayViewModel("Rename".ToLocalized());
                    var result = await OverlayService.ShowAsync(overlayViewModel);
                    if (!result.Positive())
                        return;

                    IsRenaming = true;
                    await RenameAsync(overlayViewModel.NewName, cancellationToken);
                    break;
                }
            }
        }

        [RelayCommand]
        private async Task RenameAsync(string? newName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(newName))
                newName = VaultViewModel.VaultModel.Folder.Name;

            IsRenaming = false;
            if (await VaultViewModel.VaultModel.SetVaultNameAsync(newName, cancellationToken))
                VaultViewModel.Title = newName;
        }

        [RelayCommand]
        private async Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            CustomIcon?.Dispose();
            _vaultCollectionModel.Remove(VaultViewModel.VaultModel);
            await _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.TryOpenInFileExplorerAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
        }

        private async Task UpdateIconAsync(CancellationToken cancellationToken)
        {
            var imageFile = await SafetyHelpers.NoThrowAsync(async () => await VaultViewModel.VaultModel.Folder.GetFileByNameAsync(Constants.Vault.VAULT_ICON_FILENAME, cancellationToken));
            if (imageFile is null)
                return;

            CustomIcon = await MediaService.ReadImageFileAsync(imageFile, cancellationToken);
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
