using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Sidebar
{
    public sealed class SidebarItemViewModel : ObservableObject
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultModel VaultModel { get; }

        private string? _VaultName;
        public string? VaultName
        {
            get => _VaultName;
            private set => SetProperty(ref _VaultName, value);
        }

        private DateTime _LastOpened;
        public DateTime LastOpened
        {
            get => _LastOpened;
            set => SetProperty(ref _LastOpened, value);
        }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IRelayCommand RemoveVaultCommand { get; }

        public SidebarItemViewModel(VaultModel vaultModel)
        {
            this.VaultModel = vaultModel;
            this._VaultName = vaultModel.VaultName;

            this.ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorer);
            this.RemoveVaultCommand = new RelayCommand(RemoveVault);
        }

        private async Task ShowInFileExplorer()
        {
            // TODO: Check if exists (hide the option if doesn't)
            await FileExplorerService.OpenPathInFileExplorerAsync(VaultModel!.VaultRootPath!);
        }

        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultRequestedMessage(VaultModel!));
        }
    }
}
