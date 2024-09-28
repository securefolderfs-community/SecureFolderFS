using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultService>]
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly KeyChain _keyChain;
        private readonly IVaultModel _vaultModel;
        private readonly AuthenticationType _authenticationStage;

        [ObservableProperty] private bool _CanContinue;
        [ObservableProperty] private LoginViewModel _LoginViewModel;
        [ObservableProperty] private CredentialsSelectionViewModel _SelectionViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;

        public CredentialsOverlayViewModel(IVaultModel vaultModel, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _keyChain = new();
            _vaultModel = vaultModel;
            _authenticationStage = authenticationStage;

            LoginViewModel = new(vaultModel, LoginViewType.Basic, _keyChain);
            SelectionViewModel = new(vaultModel.Folder, authenticationStage);
            SelectedViewModel = LoginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            SelectionViewModel.ConfirmationRequested += SelectionViewModel_ConfirmationRequested;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var loginMethods = VaultService.GetLoginAsync(_vaultModel.Folder, cancellationToken);
            SelectionViewModel.ConfiguredViewModel = _authenticationStage switch
            {
                AuthenticationType.FirstStageOnly => await loginMethods.FirstOrDefaultAsync(cancellationToken),
                AuthenticationType.ProceedingStageOnly => await loginMethods.ElementAtOrDefaultAsync(1, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
            };

            await SelectionViewModel.InitAsync(cancellationToken);
            await LoginViewModel.InitAsync(cancellationToken);
            CanContinue = true;
        }

        private void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            Title = "SelectAuthentication".ToLocalized();
            PrimaryButtonText = null;

            SelectionViewModel.UnlockContract = e.UnlockContract;
            SelectionViewModel.RegisterViewModel = new(_authenticationStage, _keyChain);
            SelectionViewModel.RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            SelectedViewModel = SelectionViewModel;
        }

        private void SelectionViewModel_ConfirmationRequested(object? sender, CredentialsConfirmationViewModel e)
        {
            CanContinue = e.IsRemoving || (SelectionViewModel.RegisterViewModel?.CanContinue ?? false);
            Title = e.IsRemoving ? "RemoveAuthentication".ToLocalized() : "Authenticate".ToLocalized();
            PrimaryButtonText = "Confirm".ToLocalized();
            SelectedViewModel = e;
        }

        private void RegisterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegisterViewModel.CanContinue))
                CanContinue = SelectionViewModel.RegisterViewModel?.CanContinue ?? false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (SelectionViewModel.RegisterViewModel is not null)
                SelectionViewModel.RegisterViewModel.PropertyChanged -= RegisterViewModel_PropertyChanged;

            SelectionViewModel.ConfirmationRequested -= SelectionViewModel_ConfirmationRequested;
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            SelectionViewModel.Dispose();
            LoginViewModel.Dispose();
        }
    }
}
