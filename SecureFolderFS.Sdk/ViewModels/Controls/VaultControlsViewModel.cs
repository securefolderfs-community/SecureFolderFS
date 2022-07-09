using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class VaultControlsViewModel : ObservableObject
    {
        private readonly IMessenger _messenger;
        private readonly VaultViewModel _vaultViewModel;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IAsyncRelayCommand LockVaultCommand { get; }

        public IRelayCommand OpenPropertiesCommand { get; }

        public VaultControlsViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
        {
            _messenger = messenger;
            _vaultViewModel = vaultViewModel;

            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorerAsync);
            LockVaultCommand = new AsyncRelayCommand(LockVaultAsync);
            OpenPropertiesCommand = new RelayCommand(OpenProperties);
        }

        private async Task ShowInFileExplorerAsync()
        {
            await FileExplorerService.OpenInFileExplorerAsync(_vaultViewModel.UnlockedVaultModel.RootFolder);
        }

        private async Task LockVaultAsync()
        {
            await _vaultViewModel.UnlockedVaultModel.LockAsync();
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(new VaultLoginPageViewModel(_vaultViewModel.VaultModel)));
        }

        private void OpenProperties()
        {
            _messenger.Send(new NavigationRequestedMessage(new VaultPropertiesPageViewModel(_messenger, _vaultViewModel)));
        }
    }
}
