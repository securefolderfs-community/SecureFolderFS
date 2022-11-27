using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed partial class SidebarItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultViewModel VaultViewModel { get; }

        [ObservableProperty]
        private bool _CanRemoveVault = true;

        [ObservableProperty]
        private DateTime? _LastAccessDate;

        public SidebarItemViewModel(IVaultModel vaultModel, IVaultContextModel vaultContextModel, IWidgetsContextModel widgetsContextModel)
        {
            VaultViewModel = new(vaultModel, vaultContextModel, widgetsContextModel);

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public async void Receive(VaultUnlockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
            {
                // Prevent from removing vault if it is unlocked
                CanRemoveVault = false;

                // Update last accessed date
                LastAccessDate = await VaultViewModel.VaultContextModel.GetLastAccessedDate();
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultMessage(VaultViewModel.VaultModel));
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync()
        {
            if (VaultViewModel.VaultModel.Folder is not ILocatableFolder locatableVaultFolder)
                return Task.CompletedTask;

            return FileExplorerService.OpenInFileExplorerAsync(locatableVaultFolder);
        }
    }
}
