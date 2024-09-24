using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        [ObservableProperty] private LoginViewModel _LoginViewModel;
        [ObservableProperty] private CredentialsSelectionViewModel _SelectionViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;

        public CredentialsOverlayViewModel(IVaultModel vaultModel, CredentialsSelectionViewModel selectionViewModel)
        {
            LoginViewModel = new(vaultModel, LoginViewType.Basic);
            SelectionViewModel = selectionViewModel;

            SelectedViewModel = LoginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            SelectionViewModel.ConfirmationRequested += SelectionViewModel_ConfirmationRequested;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await SelectionViewModel.InitAsync(cancellationToken);
            await LoginViewModel.InitAsync(cancellationToken);
        }

        private void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            Title = "SelectAuthentication".ToLocalized();
            PrimaryButtonText = null;
            SelectionViewModel.UnlockContract = e.UnlockContract;
            SelectedViewModel = SelectionViewModel;
        }

        private void SelectionViewModel_ConfirmationRequested(object? sender, CredentialsConfirmationViewModel e)
        {
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Confirm".ToLocalized();
            SelectedViewModel = e;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SelectionViewModel.ConfirmationRequested -= SelectionViewModel_ConfirmationRequested;
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            SelectionViewModel.Dispose();
            LoginViewModel.Dispose();
        }
    }
}
