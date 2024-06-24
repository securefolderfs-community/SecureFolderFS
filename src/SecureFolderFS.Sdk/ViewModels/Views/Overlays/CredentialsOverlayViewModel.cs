using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly IVaultModel _vaultModel;
        private readonly LoginViewModel _loginViewModel;

        [ObservableProperty] private INotifyPropertyChanged? _CurrentViewModel;

        public CredentialsOverlayViewModel(IVaultModel vaultModel)
        {
            _vaultModel = vaultModel;
            _loginViewModel = new(_vaultModel, false);

            CurrentViewModel = _loginViewModel;
            
            _loginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return _loginViewModel.InitAsync(cancellationToken);
        }

        private void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            _ = e;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _loginViewModel.Dispose();
        }
    }
}
