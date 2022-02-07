using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dialogs;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Sidebar
{
    public sealed class SidebarViewModel : ObservableObject, IRecipient<RemoveVaultRequestedMessage>
    {
        private readonly SearchModel<SidebarItemViewModel> _sidebarSearchModel;

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        public ObservableCollection<SidebarItemViewModel> SidebarItems { get; }

        private string? _SearchQuery;
        public string? SearchQuery
        {
            get => _SearchQuery;
            set
            {
                if (SetProperty(ref _SearchQuery, value))
                {
                    SearchQueryChanged(value);
                }
            }
        }

        private bool _NoItemsFoundLoad;
        public bool NoItemsFoundLoad
        {
            get => _NoItemsFoundLoad;
            set => SetProperty(ref _NoItemsFoundLoad, value);
        }

        public IAsyncRelayCommand CreateNewVaultCommand { get; }

        public IAsyncRelayCommand OpenSettingsCommand { get; }

        public SidebarViewModel()
        {
            this.SidebarItems = new();
            this.CreateNewVaultCommand = new AsyncRelayCommand(CreateNewVault);
            this.OpenSettingsCommand = new AsyncRelayCommand(OpenSettings);
            this._sidebarSearchModel = new()
            {
                Collection = SidebarItems,
                FinderPredicate = (item, key) => item.VaultName!.ToLowerInvariant().Contains(key)
            };

            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            var itemToRemove = SidebarItems.FirstOrDefault((item) => item.VaultModel == message.Value);
            if (itemToRemove != null)
            {
                SidebarItems.Remove(itemToRemove);
            }
        }

        private async Task CreateNewVault()
        {
            SearchQuery = string.Empty;

            var path = @"C:\\Temp";
            path += new Random().Next(0, 10);


            var vm = new VaultModel() { VaultRootPath = path, VaultName = Path.GetFileNameWithoutExtension(path) };
            SidebarItems.Add(new(vm));

            WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessage(vm));


            var vaultWizardViewModel = new VaultWizardDialogViewModel();
            if (await DialogService.ShowDialog(vaultWizardViewModel) == DialogResult.Primary)
            {
                if (vaultWizardViewModel.VaultModel != null)
                {
                    WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessage(vaultWizardViewModel.VaultModel));
                }
            }
        }

        private Task OpenSettings()
        {
            return Task.CompletedTask;
        }

        private void SearchQueryChanged(string? query)
        {
            NoItemsFoundLoad = !_sidebarSearchModel.SubmitQuery(query);
        }
    }
}
