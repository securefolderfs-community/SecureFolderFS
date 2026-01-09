﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>, Inject<IMediaService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class VaultListItemViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _CanCreateShortcut;
        [ObservableProperty] private bool _IsRenaming;
        [ObservableProperty] private bool _CanMoveDown;
        [ObservableProperty] private bool _CanMoveUp;
        [ObservableProperty] private bool _CanMove;
        [ObservableProperty] private IImage? _CustomIcon;
        [ObservableProperty] private VaultViewModel _VaultViewModel;

        public VaultListItemViewModel(VaultViewModel vaultViewModel, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            VaultViewModel = vaultViewModel;
            CanCreateShortcut = OperatingSystem.IsWindows();
            _vaultCollectionModel = vaultCollectionModel;

            UpdateCanMove();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await UpdateIconAsync(cancellationToken);
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
                    if (VaultViewModel.VaultModel.VaultFolder is not IModifiableFolder modifiableFolder)
                        return;

                    var sourceIconFile = await FileExplorerService.PickFileAsync(null, false, cancellationToken);
                    if (sourceIconFile is null)
                        return;

                    // TODO: Configured icon causes a crash when debugger is not attached
                    // Update vault icon
                    //var destinationIconFile = await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_ICON_FILENAME, true, cancellationToken);
                    //await sourceIconFile.CopyContentsToAsync(destinationIconFile, cancellationToken); // TODO: Resize icon (don't load large icons)
                    //await UpdateIconAsync(cancellationToken);

                    // Update folder icon
                    await using var iconStream = await sourceIconFile.OpenReadAsync(cancellationToken);
                    await MediaService.TrySetFolderIconAsync(modifiableFolder, iconStream, cancellationToken);

                    break;
                }

                // An option used on platforms where having an overlay is desirable
                case "rename":
                {
                    var overlayViewModel = new RenameOverlayViewModel("Rename".ToLocalized()) { Message = "ChooseNewName".ToLocalized(), NewName = VaultViewModel.Title };
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
                newName = VaultViewModel.VaultModel.VaultFolder?.Name;

            if (newName is null)
                return;

            IsRenaming = false;
            await VaultViewModel.SetNameAsync(newName, cancellationToken);
        }

        [RelayCommand]
        private async Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            CustomIcon?.Dispose();
            _vaultCollectionModel.Remove(VaultViewModel.VaultModel);
            if (VaultViewModel.VaultModel.VaultFolder is IBookmark bookmark)
                await bookmark.RemoveBookmarkAsync(cancellationToken);

            await _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            if (VaultViewModel.VaultModel.VaultFolder is { } vaultFolder)
                await FileExplorerService.TryOpenInFileExplorerAsync(vaultFolder, cancellationToken);
        }

        [RelayCommand]
        private async Task CreateShortcutAsync(CancellationToken cancellationToken)
        {
            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return;

            var shortcutData = new VaultShortcutDataModel(VaultViewModel.VaultModel.DataModel.PersistableId, VaultViewModel.Title);
            var suggestedName = $"{VaultViewModel.Title}{VaultService.ShortcutFileExtension}";
            await using var dataStream = await StreamSerializer.Instance.SerializeAsync(shortcutData, cancellationToken);
            
            var filter = new Dictionary<string, string>
            {
                { "SecureFolderFS Vault Shortcut", VaultService.ShortcutFileExtension }
            };

            await FileExplorerService.SaveFileAsync(suggestedName, dataStream, filter, cancellationToken);
        }

        private async Task UpdateIconAsync(CancellationToken cancellationToken)
        {
            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return;

            var imageFile = await SafetyHelpers.NoFailureAsync(async () => await vaultFolder.GetFileByNameAsync(Constants.Vault.VAULT_ICON_FILENAME, cancellationToken));
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
