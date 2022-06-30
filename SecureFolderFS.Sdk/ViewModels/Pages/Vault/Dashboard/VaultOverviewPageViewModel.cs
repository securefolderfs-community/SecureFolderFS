using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultOverviewPageViewModel : BaseDashboardPageViewModel
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        public WidgetsListViewModel WidgetsViewModel { get; }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IAsyncRelayCommand LockVaultCommand { get; }

        public IRelayCommand OpenVaultPropertiesCommand { get; }

        public VaultOverviewPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
            : base(messenger, vaultViewModel)
        {
            WidgetsViewModel = new();

            ShowInFileExplorerCommand = new AsyncRelayCommand(OpenFolderAsync);
            LockVaultCommand = new AsyncRelayCommand(LockVaultAsync);
            OpenVaultPropertiesCommand = new RelayCommand(OpenVaultProperties);
        }

        private async Task OpenFolderAsync()
        {
            if (VaultViewModel.VaultInstance is not null)
            {
                var folder = await FileSystemService.GetFolderFromPathAsync(VaultViewModel.VaultInstance.SecureFolderFSInstance.MountLocation);
                if (folder is null)
                    return;

                await FileExplorerService.OpenInFileExplorerAsync(folder);
            }
        }

        private async Task LockVaultAsync()
        {
            VaultViewModel.VaultInstance?.Dispose();
            VaultViewModel.VaultInstance = null;
            Messenger.Send(new VaultLockedMessage(VaultViewModel));
            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(VaultViewModel));
        }

        private void OpenVaultProperties()
        {
            Messenger.Send(new NavigationRequestedMessage(new VaultPropertiesPageViewModel(Messenger, VaultViewModel)));
        }
    }
}
