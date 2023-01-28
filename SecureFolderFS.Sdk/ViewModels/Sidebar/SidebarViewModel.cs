using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Shared.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed partial class SidebarViewModel : ObservableObject, IAsyncInitialize, IRecipient<AddVaultMessage>, IRecipient<RemoveVaultMessage>
    {
        private IApplicationSettingsService ApplicationSettingsService { get; } = Ioc.Default.GetRequiredService<IApplicationSettingsService>();

        private IPreferencesSettingsService PreferencesSettingsService { get; } = Ioc.Default.GetRequiredService<IPreferencesSettingsService>();

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
                if (SetProperty(ref _SelectedItem, value) && PreferencesSettingsService.ContinueOnLastVault)
                    ApplicationSettingsService.LastVaultFolderId = _SelectedItem?.VaultViewModel.VaultModel.Folder.Id;
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
                await AddVaultToSidebarAsync(item, cancellationToken);
            }

            if (PreferencesSettingsService.ContinueOnLastVault)
                SelectedItem = SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.Folder.Id.Equals(ApplicationSettingsService.LastVaultFolderId));

            SelectedItem ??= SidebarItems.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async void Receive(AddVaultMessage message)
        {
            var sidebarItem = await AddVaultToSidebarAsync(message.VaultModel);
            await VaultCollectionModel.AddVaultAsync(message.VaultModel);

            await sidebarItem.VaultViewModel.WidgetsContextModel.AddWidgetAsync(Constants.Widgets.HEALTH_WIDGET_ID);
            await sidebarItem.VaultViewModel.WidgetsContextModel.AddWidgetAsync(Constants.Widgets.GRAPHS_WIDGET_ID);
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
                // TODO This happens rarely but the vault is actually removed
            }

            SelectedItem = SidebarItems.FirstOrDefault();

            await VaultCollectionModel.RemoveVaultAsync(message.VaultModel);
        }

        private async Task<SidebarItemViewModel> AddVaultToSidebarAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default)
        {
            IVaultContextModel vaultContextModel = new SavedVaultContextModel(vaultModel);
            IWidgetsContextModel widgetsContextModel = new SavedWidgetsContextModel(vaultModel);

            var item = new SidebarItemViewModel(vaultModel, vaultContextModel, widgetsContextModel);
            item.LastAccessDate = await vaultContextModel.GetLastAccessedDate(cancellationToken);
            SidebarItems.Add(item);

            return item;
        }
    }
}
