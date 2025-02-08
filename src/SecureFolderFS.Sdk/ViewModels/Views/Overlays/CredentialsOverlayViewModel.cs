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
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultService>, Inject<IVaultCredentialsService>]
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly KeySequence _keySequence;
        private readonly IVaultModel _vaultModel;
        private readonly AuthenticationType _authenticationStage;

        [ObservableProperty] private bool _CanContinue; // TODO: Use OverlayViewModel.IsPrimaryButtonEnabled
        [ObservableProperty] private LoginViewModel _LoginViewModel;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private CredentialsSelectionViewModel _SelectionViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;

        public CredentialsOverlayViewModel(IVaultModel vaultModel, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _keySequence = new();
            _vaultModel = vaultModel;
            _authenticationStage = authenticationStage;

            RegisterViewModel = new(authenticationStage, _keySequence);
            LoginViewModel = new(vaultModel, LoginViewType.Basic, _keySequence);
            SelectionViewModel = new(vaultModel.Folder, authenticationStage);
            SelectedViewModel = LoginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            SelectionViewModel.ConfirmationRequested += SelectionViewModel_ConfirmationRequested;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var loginMethods = VaultCredentialsService.GetLoginAsync(_vaultModel.Folder, cancellationToken);
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
            if (e.IsRecovered)
            {
                Title = "SetCredentials".ToLocalized();
                PrimaryButtonText = "Confirm".ToLocalized();
                CanContinue = false;

                // Note: We can omit the fact that a flag other than FirstStage is passed to the ResetViewModel (via RegisterViewModel).
                // The flag is manipulating the order at which keys are placed in the key sequence, so it shouldn't matter if it's cleared here
                _keySequence.Dispose();
                SelectedViewModel = new CredentialsResetViewModel(_vaultModel.Folder, e.UnlockContract, RegisterViewModel).WithInitAsync();
            }
            else
            {
                Title = "SelectAuthentication".ToLocalized();
                PrimaryButtonText = null;
                SelectionViewModel.UnlockContract = e.UnlockContract;
                SelectionViewModel.RegisterViewModel = RegisterViewModel;
                SelectedViewModel = SelectionViewModel;
            }
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
                CanContinue = RegisterViewModel.CanContinue;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RegisterViewModel.PropertyChanged -= RegisterViewModel_PropertyChanged;
            SelectionViewModel.ConfirmationRequested -= SelectionViewModel_ConfirmationRequested;
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            (SelectedViewModel as IDisposable)?.Dispose();
            SelectionViewModel.Dispose();
            LoginViewModel.Dispose();
        }
    }
}
