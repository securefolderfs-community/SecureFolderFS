﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    [Inject<IFileExplorerService>]
    public sealed partial class SidebarItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        public VaultViewModel VaultViewModel { get; }

        [ObservableProperty] private bool _CanRemoveVault = true;
        [ObservableProperty] private DateTime? _LastAccessDate;

        public SidebarItemViewModel(VaultViewModel vaultViewModel, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            VaultViewModel = vaultViewModel;
            _vaultCollectionModel = vaultCollectionModel;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
            {
                // Prevent from removing vault if it is unlocked
                CanRemoveVault = false;

                // Update last accessed date
                LastAccessDate = VaultViewModel.VaultModel.LastAccessDate;
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private async Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            _vaultCollectionModel.Remove(VaultViewModel.VaultModel);
            await _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.OpenInFileExplorerAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
        }
    }
}
