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

        public VaultViewModelDeprecated VaultViewModelDeprecated { get; }

        private bool _CanRemoveVault = true;
        public bool CanRemoveVault
        {
            get => _CanRemoveVault;
            set => SetProperty(ref _CanRemoveVault, value);
        }

        public IRelayCommand RemoveVaultCommand { get; }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public SidebarItemViewModel(VaultViewModelDeprecated vaultModel)
        {
            VaultViewModelDeprecated = vaultModel;
            CanRemoveVault = true;

            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorerAsync);
            RemoveVaultCommand = new RelayCommand(RemoveVault);

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        public SidebarItemViewModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;

            RemoveVaultCommand = new RelayCommand(RemoveVault);
            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorerAsync);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (message.Value.VaultIdModel.Equals(VaultViewModelDeprecated.VaultIdModel))
                CanRemoveVault = false;
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (message.Value.VaultIdModel.Equals(VaultViewModelDeprecated.VaultIdModel))
                CanRemoveVault = true;
        }

        private void RemoveVault()
        {
            WeakReferenceMessenger.Default.Send(new RemoveVaultRequestedMessageDeprecated(VaultViewModelDeprecated.VaultIdModel));
        }

        private Task ShowInFileExplorerAsync()
        {
            return FileExplorerService.OpenPathInFileExplorerAsync(VaultViewModelDeprecated.VaultRootPath);
        }

        public bool Contains(string? other)
        {
            if (other is null)
                return false;

            return VaultViewModelDeprecated.VaultName.Contains(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
