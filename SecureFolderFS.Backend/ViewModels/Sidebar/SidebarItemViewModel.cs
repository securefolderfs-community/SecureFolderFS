using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Services;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Sidebar
{
    public sealed class SidebarItemViewModel : ObservableObject
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultViewModel VaultViewModel { get; }

        private string? _VaultName;
        public string? VaultName
        {
            get => _VaultName;
            set => SetProperty(ref _VaultName, value);
        }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IRelayCommand RemoveVaultCommand { get; }

        public SidebarItemViewModel(VaultViewModel vaultModel)
        {
            this.VaultViewModel = vaultModel;
            this._VaultName = vaultModel.VaultName;

            this.ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorer);
            this.RemoveVaultCommand = new RelayCommand(RemoveVault);
        }

        private async Task ShowInFileExplorer()
        {
            // TODO: Check if exists (hide the option if doesn't)
            await FileExplorerService.OpenPathInFileExplorerAsync(VaultViewModel.VaultRootPath);
        }

        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultRequestedMessage(VaultViewModel.VaultIdModel));
        }
    }
}
