using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Utils;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarViewModel : ObservableObject, IInitializableSource<IDictionary<VaultIdModel, VaultViewModel>>, IRecipient<RemoveVaultRequestedMessage>, IRecipient<AddVaultRequestedMessage>
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public ObservableCollection<SidebarItemViewModel> SidebarItems { get; }

        public SidebarSearchViewModel SearchViewModel { get; }

        private SidebarItemViewModel? _SelectedItem;
        public SidebarItemViewModel? SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        public IAsyncRelayCommand CreateNewVaultCommand { get; }

        public IAsyncRelayCommand OpenSettingsCommand { get; }

        public SidebarViewModel()
        {
            SidebarItems = new();
            SearchViewModel = new(SidebarItems);

            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);

            OpenSettingsCommand = new AsyncRelayCommand(OpenSettings);
            CreateNewVaultCommand = new AsyncRelayCommand(CreateNewVault);
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            var itemToRemove = SidebarItems.FirstOrDefault(item => item.VaultViewModel.VaultIdModel.Equals(message.Value));
            if (itemToRemove is not null)
            {
                SidebarItems.Remove(itemToRemove);
            }
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            SidebarItems.Add(new(message.Value));
        }

        void IInitializableSource<IDictionary<VaultIdModel, VaultViewModel>>.Initialize(IDictionary<VaultIdModel, VaultViewModel> param)
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
                    WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(itemToSelect.VaultViewModel) { Transition = new SuppressTransitionModel() });
                }
            });
        }

        private async Task CreateNewVault()
        {
            var vaultWizardViewModel = new VaultWizardDialogViewModel();
            await DialogService.ShowDialogAsync(vaultWizardViewModel);
        }

        private async Task OpenSettings()
        {
            await DialogService.ShowDialogAsync(new SettingsDialogViewModel());
        }
    }
}
