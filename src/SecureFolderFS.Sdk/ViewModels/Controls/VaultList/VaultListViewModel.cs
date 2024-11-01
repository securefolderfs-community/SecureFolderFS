using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<ISettingsService>, Inject<IIapService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class VaultListViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _HasVaults;
        [ObservableProperty] private VaultListItemViewModel? _SelectedItem;
        [ObservableProperty] private VaultListSearchViewModel _SearchViewModel;
        [ObservableProperty] private ObservableCollection<VaultListItemViewModel> _Items;

        public VaultListViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            _vaultCollectionModel = vaultCollectionModel;
            _Items = new();
            _HasVaults = true; // Assume there are vaults
            _SearchViewModel = new(Items);

            _vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
        }

        private void VaultCollectionModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewItems is not null && e.NewItems[0] is IVaultModel vaultModel:
                    AddVault(vaultModel);
                    break;

                case NotifyCollectionChangedAction.Remove when e.OldItems is not null && e.OldItems[0] is IVaultModel vaultModel:
                    RemoveVault(vaultModel);
                    break;

                case NotifyCollectionChangedAction.Move:
                    Items.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    SelectedItem = null;
                    Items.Clear();
                    HasVaults = false;
                    break;

                default: return;
            }
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            Items.Clear();
            foreach (var item in _vaultCollectionModel)
                AddVault(item);

            if (SettingsService.UserSettings.ContinueOnLastVault)
                SelectedItem = Items.FirstOrDefault(x => x.VaultViewModel.VaultModel.Folder.Id.Equals(SettingsService.AppSettings.LastVaultFolderId));

            SelectedItem ??= Items.FirstOrDefault();
            HasVaults = !Items.IsEmpty();
            
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddNewVaultAsync(IFolder? folder, CancellationToken cancellationToken)
        {
            // Check Plus version
            var isPremiumOwned = await IapService.IsOwnedAsync(IapProductType.SecureFolderFS_PlusSubscription, cancellationToken);
            if (_vaultCollectionModel.Count >= 2 && !isPremiumOwned)
            {
                _ = PaymentOverlayViewModel.Instance.InitAsync(cancellationToken);
                await OverlayService.ShowAsync(PaymentOverlayViewModel.Instance);
                return;
            }

            if (folder is not null)
            {
                // Check for duplicates
                var isDuplicate = _vaultCollectionModel.Any(x => x.Folder.Id == folder.Id);
                if (isDuplicate)
                    return;

                // Validate vault. We assume the user is adding an existing vault
                var result = await ValidationHelpers.ValidateExistingVault(folder, cancellationToken);
                if (!result.Successful)
                    return;

                // Try to save the new vault
                _vaultCollectionModel.Add(new VaultModel(folder));
                await _vaultCollectionModel.TrySaveAsync(cancellationToken);
            }
            else
            {
                using var wizardOverlay = new WizardOverlayViewModel(_vaultCollectionModel);
                await OverlayService.ShowAsync(wizardOverlay);
            }
        }

        private void AddVault(IVaultModel vaultModel)
        {
            var listItem = new VaultListItemViewModel(new(vaultModel), _vaultCollectionModel);
            Items.Add(listItem);
            HasVaults = true;
        }

        private void RemoveVault(IVaultModel vaultModel)
        {
            var itemToRemove = Items.FirstOrDefault(x => x.VaultViewModel.VaultModel == vaultModel);
            if (itemToRemove is null)
                return;

            try
            {
                Items.Remove(itemToRemove);
            }
            catch (NullReferenceException)
            {
                // This happens rarely but the vault is actually removed
            }

            SelectedItem = Items.FirstOrDefault();
            HasVaults = !Items.IsEmpty();
        }

        partial void OnSelectedItemChanged(VaultListItemViewModel? value)
        {
            if (SettingsService.UserSettings.ContinueOnLastVault)
                SettingsService.AppSettings.LastVaultFolderId = value?.VaultViewModel.VaultModel.Folder.Id;
        }
    }
}
