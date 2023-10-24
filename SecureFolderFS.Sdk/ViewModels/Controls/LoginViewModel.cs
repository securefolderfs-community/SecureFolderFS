using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Login;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>]
    public sealed partial class LoginViewModel : ObservableObject, IDisposable, IAsyncInitialize
    {
        private readonly bool _enableMigration;
        private readonly IVaultModel _vaultModel;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly IAsyncValidator<IFolder> _vaultValidator;
        private IAsyncEnumerator<AuthenticationModel>? _enumerator;

        [ObservableProperty] private BaseLoginViewModel? _CurrentViewModel;

        public LoginViewModel(IVaultModel vaultModel, bool enableMigration)
        {
            ServiceProvider = Ioc.Default;
            _enableMigration = enableMigration;
            _vaultModel = vaultModel;

            _vaultValidator = VaultService.GetVaultValidator();
            _vaultWatcherModel = new VaultWatcherModel(vaultModel.Folder);
            _vaultWatcherModel.StateChanged += VaultWatcherModel_StateChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get the authentication method enumerator for this vault
            _enumerator = VaultService.VaultAuthenticator.GetAuthenticationAsync(_vaultModel.Folder, cancellationToken).GetAsyncEnumerator(cancellationToken);

            var validator = VaultService.GetVaultValidator();
            var validationResult = await validator.TryValidateAsync(_vaultModel.Folder, cancellationToken);

            if (validationResult.Successful)
            {
                // Set up the first authentication method
                if (!await TryNextAuthAsync())
                    CurrentViewModel = new ErrorViewModel("No authentication methods available.");
            }
            else
            {
                // Try to migrate the vault if not supported
                if (_enableMigration && validationResult.Exception is NotSupportedException)
                    CurrentViewModel = new MigrationViewModel();
                else
                    CurrentViewModel = new ErrorViewModel("Cannot migrate vault.");
            }
        }

        private async void VaultWatcherModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is VaultChangedEventArgs { ContentsChanged: true })
                await ValidateVaultAsync();
        }

        private async Task ValidateVaultAsync(CancellationToken cancellationToken = default)
        {
            var result = await _vaultValidator.TryValidateAsync(_vaultModel.Folder, cancellationToken);
            if (result.Successful)
                return;

            CurrentViewModel = new ErrorViewModel(result.GetMessage());
        }

        private async Task TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var credentials = _credentialsBuilder.BuildCredentials();
                var lifetimeModel = await VaultService.VaultUnlocker.UnlockAsync(_vaultModel, credentials, cancellationToken);

                // Update last access date
                await _vaultModel.SetLastAccessDateAsync(DateTime.Now, cancellationToken);

                // TODO(1) CUT HERE - LoginViewModel should only handle unlocking views. The unlocked model should be forwarded back to the VM that actually manages LoginVM (instead of navigating here)

                // Create view models
                var unlockedVaultViewModel = new UnlockedVaultViewModel(VaultViewModel, lifetimeModel);
                var dashboardPage = new VaultDashboardPageViewModel(unlockedVaultViewModel, NavigationService);

                // Notify that the vault has been unlocked
                WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(_vaultModel));

                // Dispose current instance and navigate
                Dispose();
                await NavigationService.TryNavigateAndForgetAsync(dashboardPage);
            }
            catch (Exception ex)
            {
                // If failed, restart the process
                // TODO: Above ^
                _ = ex;
            }
        }

        private async void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is not AuthenticationChangedEventArgs args)
                return;

            // Add authentication
            _credentialsBuilder.Add(args.Authentication);

            if (!await TryNextAuthAsync() && LoginTypeViewModel is not ErrorViewModel)
            {
                // Reached the end, in which case we should try to unlock the vault
                await TryUnlockAsync();
            }
        }

        private async Task<bool> TryNextAuthAsync()
        {
            if (_enumerator is null || !await _enumerator.MoveNextAsync())
                return false;

            // Get the appropriate method
            var authenticationModel = _enumerator.Current;
            LoginTypeViewModel = authenticationModel.AuthenticationType switch
            {
                AuthenticationType.Password => new PasswordViewModel(),
                AuthenticationType.Other => new AuthenticationViewModel(authenticationModel),
                _ => new ErrorViewModel("Could not determine the authentication type.")
            };

            return true;
        }

        partial void OnCurrentViewModelChanging(BaseLoginViewModel? oldValue, BaseLoginViewModel? newValue)
        {
            // Unhook old
            if (oldValue is not null)
                oldValue.StateChanged -= CurrentViewModel_StateChanged;

            // Hook up new
            if (newValue is not null)
                newValue.StateChanged += CurrentViewModel_StateChanged;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
        }
    }
}
