using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed partial class SidebarItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public IVaultModel VaultModel { get; }

        [ObservableProperty]
        private bool _CanRemoveVault = true;

        public SidebarItemViewModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (VaultModel.Equals(message.VaultModel))
                CanRemoveVault = false;
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultMessage(VaultModel));
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync()
        {
            if (VaultModel.Folder is not ILocatableFolder vaultFolder)
                return Task.CompletedTask;

            return FileExplorerService.OpenInFileExplorerAsync(vaultFolder);
        }
    }
}
