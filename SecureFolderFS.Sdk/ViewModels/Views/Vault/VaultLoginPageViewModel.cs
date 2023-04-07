using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<VaultUnlockedMessage>
    {
        private readonly IVaultLoginModel _vaultLoginModel;

        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private INotifyPropertyChanged? _StrategyViewModel;

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationModel navigationModel)
            : base(vaultViewModel)
        {
            VaultName = vaultViewModel.VaultModel.VaultName;
            _vaultLoginModel = new VaultLoginModel(vaultViewModel.VaultModel, new VaultWatcherModel(vaultViewModel.VaultModel.Folder));
            _vaultLoginModel.StrategyChanged += VaultLoginModel_StrategyChanged;

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _vaultLoginModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            // Free resources that are no longer being used for login
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        private void VaultLoginModel_StrategyChanged(object? sender, IVaultStrategyModel e)
        {
            (StrategyViewModel as IDisposable)?.Dispose();
            StrategyViewModel = new 
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _vaultLoginModel.Dispose();
            _vaultLoginModel.StrategyChanged -= VaultLoginModel_StrategyChanged;
            (StrategyViewModel as IDisposable)?.Dispose();
        }
    }
}
