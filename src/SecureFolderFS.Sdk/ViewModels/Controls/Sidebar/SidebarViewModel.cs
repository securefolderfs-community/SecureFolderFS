using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    [Inject<ISettingsService>]
    public sealed partial class SidebarViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private SidebarItemViewModel? _SelectedItem;
        [ObservableProperty] private SidebarSearchViewModel _SearchViewModel;
        [ObservableProperty] private SidebarFooterViewModel _FooterViewModel;
        [ObservableProperty] private ObservableCollection<SidebarItemViewModel> _SidebarItems;

        public SidebarViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCollectionModel = vaultCollectionModel;
            _SidebarItems = new();
            _SearchViewModel = new(SidebarItems);
            _FooterViewModel = new(_vaultCollectionModel);

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
                    SidebarItems.Clear();
                    break;

                default:
                    return;
            }
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            SidebarItems.Clear();
            foreach (var item in _vaultCollectionModel)
                AddVault(item);

            if (SettingsService.UserSettings.ContinueOnLastVault)
                SelectedItem = SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.Folder.Id.Equals(SettingsService.AppSettings.LastVaultFolderId));

            SelectedItem ??= SidebarItems.FirstOrDefault();
            return Task.CompletedTask;
        }

        private void AddVault(IVaultModel vaultModel)
        {
            var widgetsCollection = new WidgetsCollectionModel(vaultModel.Folder);
            var vaultViewModel = new VaultViewModel(vaultModel, widgetsCollection);

            var sidebarItem = new SidebarItemViewModel(vaultViewModel, _vaultCollectionModel);

            sidebarItem.LastAccessDate = vaultModel.LastAccessDate;
            SidebarItems.Add(sidebarItem);
        }

        private void RemoveVault(IVaultModel vaultModel)
        {
            var itemToRemove = SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel == vaultModel);
            if (itemToRemove is null)
                return;

            try
            {
                SidebarItems.Remove(itemToRemove);
            }
            catch (NullReferenceException)
            {
                // TODO: This happens rarely but the vault is actually removed
            }

            SelectedItem = SidebarItems.FirstOrDefault();
        }

        partial void OnSelectedItemChanged(SidebarItemViewModel? value)
        {
            if (SettingsService.UserSettings.ContinueOnLastVault)
                SettingsService.AppSettings.LastVaultFolderId = value?.VaultViewModel.VaultModel.Folder.Id;
        }
    }
}
