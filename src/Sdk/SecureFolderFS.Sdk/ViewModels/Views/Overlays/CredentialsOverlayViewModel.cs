using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultService>, Inject<IVaultCredentialsService>]
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly KeySequence _loginKeySequence;
        private readonly KeySequence _registerKeySequence;
        private readonly AuthenticationStage _authenticationStage;

        [ObservableProperty] private LoginViewModel _LoginViewModel;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private CredentialsSelectionViewModel _SelectionViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;

        public CredentialsOverlayViewModel(IFolder vaultFolder, string? vaultName, AuthenticationStage authenticationStage)
        {
            ServiceProvider = DI.Default;
            _loginKeySequence = new();
            _registerKeySequence = new();
            _vaultFolder = vaultFolder;
            _authenticationStage = authenticationStage;

            RegisterViewModel = new(authenticationStage, _registerKeySequence);
            LoginViewModel = new(vaultFolder, LoginViewType.Basic, _loginKeySequence) { Title = vaultName };
            SelectionViewModel = new(vaultFolder, authenticationStage);
            SelectedViewModel = LoginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryText = "Continue".ToLocalized();

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            SelectionViewModel.ConfirmationRequested += SelectionViewModel_ConfirmationRequested;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var loginItems = await VaultCredentialsService.GetLoginAsync(_vaultFolder, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                SelectionViewModel.ConfiguredViewModel = _authenticationStage switch
                {
                    AuthenticationStage.FirstStageOnly => loginItems.FirstOrDefault(),
                    AuthenticationStage.ProceedingStageOnly => loginItems.ElementAtOrDefault(1),
                    _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
                };
            }
            catch (Exception ex)
            {
                // If an unsupported authentication method is configured, don't offer to modify it
                _ = ex;
            }

            await SelectionViewModel.InitAsync(cancellationToken);
            await LoginViewModel.InitAsync(cancellationToken);
            CanContinue = true;
        }

        private void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            if (e.IsRecovered)
            {
                Title = "SetCredentials".ToLocalized();
                PrimaryText = "Confirm".ToLocalized();
                CanContinue = false;

                // Note: We can omit the fact that a flag other than FirstStage is passed to the ResetViewModel (via RegisterViewModel).
                // The flag is manipulating the order at which keys are placed in the key sequence, so it shouldn't matter if it's cleared here
                _loginKeySequence.Dispose();
                SelectedViewModel = new CredentialsResetViewModel(_vaultFolder, e.UnlockContract, RegisterViewModel).WithInitAsync();
            }
            else
            {
                Title = "SelectAuthentication".ToLocalized();
                PrimaryText = null;
                SelectionViewModel.UnlockContract = e.UnlockContract;
                SelectionViewModel.OldPasskey = _loginKeySequence;

                // Seed the register sequence with the already-authenticated first-stage key
                // so that when a second-stage method is added, the combined passkey is complete
                foreach (var key in _loginKeySequence.Keys)
                    _registerKeySequence.SetOrAdd(0, key); // First-stage lives at index 0

                SelectionViewModel.RegisterViewModel = RegisterViewModel;
                SelectedViewModel = SelectionViewModel;
            }
        }

        private void SelectionViewModel_ConfirmationRequested(object? sender, CredentialsConfirmationViewModel e)
        {
            CanContinue = e.IsRemoving || (SelectionViewModel.RegisterViewModel?.CanContinue ?? false);
            Title = e.IsRemoving ? "RemoveAuthentication".ToLocalized() : "Authenticate".ToLocalized();
            PrimaryText = "Confirm".ToLocalized();
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
            _loginKeySequence.Dispose();
            _registerKeySequence.Dispose();
        }
    }
}
