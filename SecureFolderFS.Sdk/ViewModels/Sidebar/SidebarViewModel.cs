using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.Utils;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarViewModel : ObservableObject, 
        IAsyncInitialize, 
        IInitializableSource<IDictionary<VaultIdModel, VaultViewModelDeprecated>>, 
        IRecipient<RemoveVaultRequestedMessageDeprecated>, 
        IRecipient<AddVaultRequestedMessageDeprecated>,
        IRecipient<AddVaultMessage>,
        IRecipient<RemoveVaultMessage>
    {
        private IVaultCollectionModel VaultCollectionModel { get; }

        public ObservableCollection<SidebarItemViewModel> SidebarItems { get; }

        public SidebarSearchViewModel SearchViewModel { get; }

        public SidebarFooterViewModel FooterViewModel { get; }

        private SidebarItemViewModel? _SelectedItem;
        public SidebarItemViewModel? SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        public SidebarViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            VaultCollectionModel = vaultCollectionModel;
            SidebarItems = new();
            SearchViewModel = new(SidebarItems);
            FooterViewModel = new();

            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessageDeprecated>(this);
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessageDeprecated>(this);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in VaultCollectionModel.GetVaultsAsync(cancellationToken))
            {
                SidebarItems.Add(new(item));
            }
        }

        /// <inheritdoc/>
        public void Receive(RemoveVaultRequestedMessageDeprecated message)
        {
            var itemToRemove = SidebarItems.FirstOrDefault(item => item.VaultViewModelDeprecated.VaultIdModel.Equals(message.Value));
            if (itemToRemove is not null)
            {
                SidebarItems.Remove(itemToRemove);
                SelectedItem = SidebarItems.FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public void Receive(AddVaultRequestedMessageDeprecated message)
        {
            SidebarItems.Add(new(message.Value));
        }

        void IInitializableSource<IDictionary<VaultIdModel, VaultViewModelDeprecated>>.Initialize(IDictionary<VaultIdModel, VaultViewModelDeprecated> param)
        {
            _ = ThreadingService.ExecuteOnUiThreadAsync(() =>
            {
                foreach (var item in param.Values)
                {
                    SidebarItems.Add(new(item));
                }

                if (SidebarItems.FirstOrDefault() is SidebarItemViewModel itemToSelect)
                {
                    SelectedItem = itemToSelect;
                    WeakReferenceMessenger.Default.Send(new VaultNavigationRequestedMessage(itemToSelect.VaultViewModelDeprecated) { Transition = new SuppressTransitionModel() });
                }
            });
        }

        /// <inheritdoc/>
        public async void Receive(AddVaultMessage message)
        {
            SidebarItems.Add(new(message.VaultModel));
            await VaultCollectionModel.AddVaultAsync(message.VaultModel);
        }

        /// <inheritdoc/>
        public async void Receive(RemoveVaultMessage message)
        {
            var itemToRemove = SidebarItems.FirstOrDefault(x => x.VaultModel.Equals(message.VaultModel));
            if (itemToRemove is null)
                return;

            SidebarItems.Remove(itemToRemove);
            await VaultCollectionModel.RemoveVaultAsync(message.VaultModel);
        }
    }
}
