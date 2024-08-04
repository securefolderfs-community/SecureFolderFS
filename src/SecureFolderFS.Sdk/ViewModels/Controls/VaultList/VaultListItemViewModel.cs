using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultListItemViewModel : ObservableObject, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private bool _CanRemoveVault = true;
        [ObservableProperty] private DateTime? _LastAccessDate;

        public IVaultModel VaultModel { get; }

        public VaultListItemViewModel(IVaultModel vaultModel, IVaultCollectionModel vaultCollectionModel)
        {
            _vaultCollectionModel = vaultCollectionModel;
            LastAccessDate = vaultModel.LastAccessDate;
            VaultModel = vaultModel;
            ServiceProvider = DI.Default;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (VaultModel.Equals(message.VaultModel))
            {
                // Prevent from removing vault if it is unlocked
                CanRemoveVault = false;

                // Update last accessed date
                LastAccessDate = VaultModel.LastAccessDate;
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultModel.Equals(message.VaultModel))
                CanRemoveVault = true;
        }

        [RelayCommand]
        private Task RemoveVaultAsync(CancellationToken cancellationToken)
        {
            _vaultCollectionModel.Remove(VaultModel);
            return _vaultCollectionModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        private Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.TryOpenInFileExplorerAsync(VaultModel.Folder, cancellationToken);
        }
    }
}
