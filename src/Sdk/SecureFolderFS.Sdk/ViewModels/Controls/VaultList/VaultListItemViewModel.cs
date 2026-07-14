using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
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

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>, Inject<IMediaService>, Inject<IVaultService>, Inject<IIapService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class VaultListItemViewModel : ObservableObject, IAsyncInitialize, IDisposable
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
            SettingsService.UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        public bool IsAutoUnlockEnabled
        {
            get
            {
                var persistableId = VaultViewModel.VaultModel.DataModel.PersistableId;
                return persistableId is not null && persistableId.Equals(SettingsService.UserSettings.AutoUnlockVaultId);
            }
            set
            {
                var persistableId = VaultViewModel.VaultModel.DataModel.PersistableId;
                if (persistableId is null)
                    return;

                if (value)
                {
                    // Marking this vault automatically unmarks the previously chosen one, since only one vault can participate at a time
                    SettingsService.UserSettings.AutoUnlockVaultId = persistableId;
                }
                else if (persistableId.Equals(SettingsService.UserSettings.AutoUnlockVaultId))
                    SettingsService.UserSettings.AutoUnlockVaultId = null;

                _ = SettingsService.UserSettings.TrySaveAsync();
            }
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
            await _vaultCollectionModel.TrySaveAsync(cancellationToken);
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

                    // Guard the icon update so a decode/IO failure (e.g., an unsupported or oversized image)
                    // degrades gracefully instead of crashing the command
                    try
                    {
                        await using var iconStream = await sourceIconFile.OpenReadAsync(cancellationToken);
                        await MediaService.TrySetFolderIconAsync(modifiableFolder, iconStream, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation. Nothing to report
                    }
                    catch (Exception ex)
                    {
                        DI.OptionalService<ILogger>()?.LogError(ex, "Failed to set the vault folder icon.");
                    }

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
            // Unmark the vault from auto unlock when it is removed from the list
            if (IsAutoUnlockEnabled)
                IsAutoUnlockEnabled = false;

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
            if (VaultViewModel.VaultModel.VaultFolder is null)
                return;

            // Check Iap Plus requirement
            if (!await IapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
            {
                await OverlayService.ShowAsync(PaymentOverlayViewModel.Instance.WithInitAsync(cancellationToken));
                if (!await IapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
                    return;
            }

            var shortcutData = new VaultShortcutDataModel(VaultViewModel.VaultModel.DataModel.PersistableId, VaultViewModel.Title);
            var suggestedName = $"{VaultViewModel.Title}{VaultService.ShortcutFileExtension}";
            await using var dataStream = await StreamSerializer.Instance.SerializeAsync(shortcutData, cancellationToken);

            var filter = new Dictionary<string, string>
            {
                { $"{"Vault".ToLocalized()} ({nameof(SecureFolderFS)})", VaultService.ShortcutFileExtension }
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

        private void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Update the check state of every item when another vault is chosen for auto unlock
            if (e.PropertyName == nameof(SettingsService.UserSettings.AutoUnlockVaultId))
                OnPropertyChanged(nameof(IsAutoUnlockEnabled));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SettingsService.UserSettings.PropertyChanged -= UserSettings_PropertyChanged;
        }
    }
}
