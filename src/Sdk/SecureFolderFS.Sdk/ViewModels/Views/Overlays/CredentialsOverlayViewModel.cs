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
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IProgress<IResult>, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly KeySequence _loginKeySequence;
        private readonly KeySequence _registerKeySequence;
        private readonly AuthenticationStage _authenticationStage;
        private string? _primaryAuthenticationMethodId;

        [ObservableProperty] private LoginViewModel _LoginViewModel;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private CredentialsSelectionViewModel _SelectionViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;
        [ObservableProperty] private InfoBarViewModel _StatusInfoBar = new();

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
            LoginViewModel.StateChanged += LoginViewModel_StateChanged;
            RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            SelectionViewModel.ConfirmationRequested += SelectionViewModel_ConfirmationRequested;
        }

        /// <inheritdoc/>
        public void Report(IResult result)
        {
            if (result.Successful)
            {
                StatusInfoBar.IsOpen = false;
                return;
            }

            StatusInfoBar.Title = "CredentialsChangeFailed".ToLocalized();
            StatusInfoBar.Message = result.GetMessage("UnknownError".ToLocalized());
            StatusInfoBar.Severity = Severity.Critical;
            StatusInfoBar.IsCloseable = true;
            StatusInfoBar.IsOpen = true;
        }

        private void LoginViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is ErrorReportedEventArgs args)
                Report(args.Result);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
                _primaryAuthenticationMethodId = vaultOptions.UnlockProcedure.Methods.FirstOrDefault();
                LoginViewModel.RequiredAuthenticationMethodIds = GetRequiredAuthenticationMethodIds(vaultOptions.UnlockProcedure);

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
                SelectionViewModel.OldAuthenticationMethodIds = LoginViewModel.AuthenticatedMethodIds.ToArray();

                // Seed the register sequence with the already-authenticated first-stage key
                // so that when a second-stage method is added, the combined passkey is complete
                var firstAuthenticatedMethodId = LoginViewModel.AuthenticatedMethodIds.FirstOrDefault();
                var firstStageKey = string.Equals(firstAuthenticatedMethodId, _primaryAuthenticationMethodId, StringComparison.Ordinal)
                    ? _loginKeySequence.Keys.FirstOrDefault()
                    : null;
                if (firstStageKey is not null)
                    _registerKeySequence.SetOrAdd(0, firstStageKey); // First-stage lives at index 0

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

        private string[]? GetRequiredAuthenticationMethodIds(AuthenticationMethod unlockProcedure)
        {
            if (string.IsNullOrWhiteSpace(unlockProcedure.Complementation))
                return null;

            return _authenticationStage switch
            {
                AuthenticationStage.FirstStageOnly => [ unlockProcedure.Complementation ],
                AuthenticationStage.ProceedingStageOnly => _primaryAuthenticationMethodId is null ? [] : [ _primaryAuthenticationMethodId ],
                _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RegisterViewModel.PropertyChanged -= RegisterViewModel_PropertyChanged;
            SelectionViewModel.ConfirmationRequested -= SelectionViewModel_ConfirmationRequested;
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            LoginViewModel.StateChanged -= LoginViewModel_StateChanged;
            (SelectedViewModel as IDisposable)?.Dispose();
            SelectionViewModel.Dispose();
            LoginViewModel.Dispose();
            RegisterViewModel.Dispose();
            _loginKeySequence.Dispose();
            _registerKeySequence.Dispose();
        }
    }
}
