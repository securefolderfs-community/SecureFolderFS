using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<ISettingsService>, Inject<IIapService>, Inject<IOverlayService>]
    public sealed partial class VaultListViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private VaultListItemViewModel? _SelectedItem;
        [ObservableProperty] private VaultListSearchViewModel _SearchViewModel;
        [ObservableProperty] private ObservableCollection<VaultListItemViewModel> _Items;

        public VaultListViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCollectionModel = vaultCollectionModel;
            _Items = new();
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

                case NotifyCollectionChangedAction.Reset:
                    SelectedItem = null;
                    Items.Clear();
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
                SelectedItem = Items.FirstOrDefault(x => x.VaultModel.Folder.Id.Equals(SettingsService.AppSettings.LastVaultFolderId));

            SelectedItem ??= Items.FirstOrDefault();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddNewVaultAsync(CancellationToken cancellationToken)
        {
            var isPremiumOwned = await IapService.IsOwnedAsync(IapProductType.SecureFolderFSPlus, cancellationToken);
            if (_vaultCollectionModel.Count >= 2 && !isPremiumOwned)
            {
                _ = PaymentDialogViewModel.Instance.InitAsync(cancellationToken);
                await OverlayService.ShowAsync(PaymentDialogViewModel.Instance);
            }
            else
                await OverlayService.ShowAsync(new WizardOverlayViewModel(_vaultCollectionModel));
        }

        [RelayCommand]
        private async Task OpenSettingsAsync(CancellationToken cancellationToken)
        {
            await OverlayService.ShowAsync(SettingsDialogViewModel.Instance);
            await SettingsService.TrySaveAsync(cancellationToken);
        }

        private void AddVault(IVaultModel vaultModel)
        {
            var widgetsCollection = new WidgetsCollectionModel(vaultModel.Folder);
            var listItem = new VaultListItemViewModel(vaultModel, _vaultCollectionModel);

            listItem.LastAccessDate = vaultModel.LastAccessDate;
            Items.Add(listItem);
        }

        private void RemoveVault(IVaultModel vaultModel)
        {
            var itemToRemove = Items.FirstOrDefault(x => x.VaultModel == vaultModel);
            if (itemToRemove is null)
                return;

            try
            {
                Items.Remove(itemToRemove);
            }
            catch (NullReferenceException)
            {
                // TODO: This happens rarely but the vault is actually removed
            }

            SelectedItem = Items.FirstOrDefault();
        }

        partial void OnSelectedItemChanged(VaultListItemViewModel? value)
        {
            if (SettingsService.UserSettings.ContinueOnLastVault)
                SettingsService.AppSettings.LastVaultFolderId = value?.VaultModel.Folder.Id;
        }
    }
}
