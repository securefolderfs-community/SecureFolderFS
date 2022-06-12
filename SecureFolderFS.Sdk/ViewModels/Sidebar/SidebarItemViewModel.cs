using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultViewModel VaultViewModel { get; }

        private string? _VaultName;
        public string? VaultName
        {
            get => _VaultName;
            set => SetProperty(ref _VaultName, value);
        }

        private bool _CanRemoveVault;
        public bool CanRemoveVault
        {
            get => _CanRemoveVault;
            set => SetProperty(ref _CanRemoveVault, value);
        }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IRelayCommand RemoveVaultCommand { get; }

        public SidebarItemViewModel(VaultViewModel vaultModel)
        {
            VaultViewModel = vaultModel;
            _VaultName = vaultModel.VaultName;
            _CanRemoveVault = true;

            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorer);
            RemoveVaultCommand = new RelayCommand(RemoveVault);

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        public void Receive(VaultUnlockedMessage message)
        {
            if (message.Value.VaultIdModel.Equals(VaultViewModel.VaultIdModel))
            {
                CanRemoveVault = false;
            }
        }

        public void Receive(VaultLockedMessage message)
        {
            if (message.Value.VaultIdModel.Equals(VaultViewModel.VaultIdModel))
            {
                CanRemoveVault = true;
            }
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
