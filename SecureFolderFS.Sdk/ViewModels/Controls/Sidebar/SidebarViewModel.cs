using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    public sealed partial class SidebarViewModel : ObservableObject, IAsyncInitialize, IRecipient<AddVaultMessage>, IRecipient<RemoveVaultMessage>
    {
        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        private IVaultCollectionModel VaultCollectionModel { get; }

        public ObservableCollection<SidebarItemViewModel> SidebarItems { get; }

        public SidebarSearchViewModel SearchViewModel { get; }

        public SidebarFooterViewModel FooterViewModel { get; }

        private SidebarItemViewModel? _SelectedItem;
        public SidebarItemViewModel? SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (SetProperty(ref _SelectedItem, value) && SettingsService.UserSettings.ContinueOnLastVault)
                    SettingsService.AppSettings.LastVaultFolderId = _SelectedItem?.VaultViewModel.VaultModel.Folder.Id;
            }
        }

        public SidebarViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            VaultCollectionModel = vaultCollectionModel;
            SidebarItems = new();
            SearchViewModel = new(SidebarItems);
            FooterViewModel = new();

            WeakReferenceMessenger.Default.Register<AddVaultMessage>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultMessage>(this);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in VaultCollectionModel.GetVaultsAsync(cancellationToken))
            {
                AddVaultToSidebar(item);
            }

            if (SettingsService.UserSettings.ContinueOnLastVault)
                SelectedItem = SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.Folder.Id.Equals(SettingsService.AppSettings.LastVaultFolderId));

            SelectedItem ??= SidebarItems.FirstOrDefault();
        }

        /// <inheritdoc/>
        public void Receive(AddVaultMessage message)
        {
            AddVaultToSidebar(message.VaultModel);
        }

        /// <inheritdoc/>
        public async void Receive(RemoveVaultMessage message)
        {
            var itemToRemove = SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.Equals(message.VaultModel));
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
            await VaultCollectionModel.RemoveVaultAsync(message.VaultModel);
        }

        private void AddVaultToSidebar(IVaultModel vaultModel)
        {
            var widgetsCollection = new WidgetsCollectionModel(vaultModel.Folder);
            var sidebarItem = new SidebarItemViewModel(vaultModel, widgetsCollection);

            sidebarItem.LastAccessDate = vaultModel.LastAccessDate;
            SidebarItems.Add(sidebarItem);
        }
    }
}
