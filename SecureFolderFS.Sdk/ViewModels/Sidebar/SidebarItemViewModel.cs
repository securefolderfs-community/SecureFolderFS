using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarItemViewModel : ObservableObject, IContainable<string>, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public IVaultModel VaultModel { get; }

        private bool _CanRemoveVault = true;
        public bool CanRemoveVault
        {
            get => _CanRemoveVault;
            set => SetProperty(ref _CanRemoveVault, value);
        }

        public IRelayCommand RemoveVaultCommand { get; }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public SidebarItemViewModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;

            RemoveVaultCommand = new RelayCommand(RemoveVault);
            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorerAsync);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            // TODO: Update CanRemoveVault
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            // TODO: Update CanRemoveVault
        }

        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultMessage(VaultModel));
        }

        private Task ShowInFileExplorerAsync()
        {
            return FileExplorerService.OpenInFileExplorerAsync(VaultModel.Folder);
        }

        /// <inheritdoc/>
        public bool Contains(string? other)
        {
            if (other is null)
                return false;

            return VaultModel.VaultName.Contains(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
