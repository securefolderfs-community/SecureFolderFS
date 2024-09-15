﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>, Inject<IVaultManagerService>]
    [Bindable(true)]
    public sealed partial class LoginViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly LoginViewType _loginViewMode;
        private readonly IVaultModel _vaultModel;
        private readonly KeyChain _keyChain;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private IAsyncEnumerator<AuthenticationViewModel>? _enumerator;

        [ObservableProperty] private ICommand? _ProvideCredentialsCommand;
        [ObservableProperty] private ReportableViewModel? _CurrentViewModel;

        public event EventHandler<VaultUnlockedEventArgs>? VaultUnlocked;

        public LoginViewModel(IVaultModel vaultModel, LoginViewType loginViewMode)
        {
            ServiceProvider = DI.Default;
            _loginViewMode = loginViewMode;
            _vaultModel = vaultModel;
            _keyChain = new();
            _vaultWatcherModel = new VaultWatcherModel(vaultModel.Folder);
            _vaultWatcherModel.StateChanged += VaultWatcherModel_StateChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get the authentication method enumerator for this vault
            _enumerator = VaultService.GetLoginAsync(_vaultModel.Folder, cancellationToken).GetAsyncEnumerator(cancellationToken);

            var validationResult = await VaultService.VaultValidator.TryValidateAsync(_vaultModel.Folder, cancellationToken);
            if (validationResult.Successful)
            {
                // Set up the first authentication method
                if (!await TryNextAuthAsync())
                    CurrentViewModel = new ErrorViewModel("No authentication methods available.", null);
            }
            else
            {
                // Try to migrate the vault if not supported
                if (validationResult.Exception is NotSupportedException ex && ex.Data["Version"] is int currentVersion)
                {
                    CurrentViewModel = _loginViewMode switch
                    {
                        LoginViewType.Full => new MigrationViewModel(_vaultModel, currentVersion),
                        _ => new ErrorViewModel("You'll need to migrate this vault before it can be used.", null)
                    };
                }
                else
                    CurrentViewModel = new ErrorViewModel(null, validationResult.GetMessage());
            }

            // TODO: VaultWatcherModel.InitAsync is never called
        }

        [RelayCommand]
        private async Task RecoverAccessAsync(string? masterKey, CancellationToken cancellationToken)
        {
            if (masterKey is null)
                return;

            try
            {
                var unlockContract = await VaultManagerService.RecoverAsync(_vaultModel.Folder, masterKey, cancellationToken);
                VaultUnlocked?.Invoke(this, new(unlockContract, _vaultModel.Folder));
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        private async Task TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var unlockContract = await VaultManagerService.UnlockAsync(_vaultModel.Folder, _keyChain, cancellationToken);
                VaultUnlocked?.Invoke(this, new(unlockContract, _vaultModel.Folder));
            }
            catch (Exception ex)
            {
                // Report that an error occurred when trying to log in
                CurrentViewModel?.Report(Result.Failure(ex));
            }
            finally
            {
                _keyChain.Dispose();
            }
        }

        private async Task<bool> TryNextAuthAsync()
        {
            if (_enumerator is null || !await _enumerator.MoveNextAsync())
                return false;

            // Get the appropriate method
            var viewModel = _enumerator.Current;
            CurrentViewModel = viewModel;

            return true;
        }

        private async void VaultWatcherModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is VaultChangedEventArgs { ContentsChanged: true })
            {
                var result = await VaultService.VaultValidator.TryValidateAsync(_vaultModel.Folder);
                if (result.Successful)
                    return;

                CurrentViewModel = new ErrorViewModel(null, result.GetMessage());
            }
        }

        private async void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is CredentialsProvisionChangedEventArgs provisionArgs)
            {
                // TODO
                _ = provisionArgs.ClearProvision;
                _ = provisionArgs.SignedProvision;
            }
            else if (e is MigrationCompletedEventArgs)
            {
                _keyChain.Dispose();
                if (_enumerator is not null)
                    await _enumerator.DisposeAsync();

                await InitAsync();
            }
        }

        private async void CurrentViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            // Add authentication
            _keyChain.Push(e.Authentication);

            if (!await TryNextAuthAsync() && CurrentViewModel is not ErrorViewModel)
            {
                // Reached the end in which case we should try to unlock the vault
                await TryUnlockAsync();
            }
        }

        partial void OnCurrentViewModelChanging(ReportableViewModel? oldValue, ReportableViewModel? newValue)
        {
            // Detach old
            if (oldValue is not null)
                oldValue.StateChanged -= CurrentViewModel_StateChanged;

            if (oldValue is AuthenticationViewModel oldViewModel)
                oldViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

            // Attach new
            if (newValue is not null)
                newValue.StateChanged += CurrentViewModel_StateChanged;

            if (newValue is AuthenticationViewModel newViewModel)
            {
                newViewModel.CredentialsProvided += CurrentViewModel_CredentialsProvided;
                ProvideCredentialsCommand = newViewModel.ProvideCredentialsCommand;
            }
            else
                ProvideCredentialsCommand = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultUnlocked = null;
            _vaultWatcherModel.StateChanged -= VaultWatcherModel_StateChanged;

            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;

            if (CurrentViewModel is AuthenticationViewModel authenticationViewModel)
                authenticationViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

            _keyChain.Dispose();
            _ = _enumerator?.DisposeAsync().ConfigureAwait(false);
        }
    }
}
