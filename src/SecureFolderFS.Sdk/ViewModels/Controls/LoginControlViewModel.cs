﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>, Inject<IVaultManagerService>]
    public sealed partial class LoginControlViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly bool _enableMigration;
        private readonly IVaultModel _vaultModel;
        private readonly CredentialsModel _credentials;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private IAsyncEnumerator<AuthenticationViewModel>? _enumerator;

        [ObservableProperty] private ReportableViewModel? _CurrentViewModel;

        public event EventHandler<VaultUnlockedEventArgs>? VaultUnlocked;

        public LoginControlViewModel(IVaultModel vaultModel, bool enableMigration)
        {
            ServiceProvider = Ioc.Default;
            _enableMigration = enableMigration;
            _vaultModel = vaultModel;
            _credentials = new();
            _vaultWatcherModel = new VaultWatcherModel(vaultModel.Folder);
            _vaultWatcherModel.StateChanged += VaultWatcherModel_StateChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get the authentication method enumerator for this vault
            _enumerator = VaultManagerService.GetLoginAuthenticationAsync(_vaultModel.Folder, cancellationToken).GetAsyncEnumerator(cancellationToken);

            var validationResult = await VaultService.VaultValidator.TryValidateAsync(_vaultModel.Folder, cancellationToken);
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
                    CurrentViewModel = new MigrationViewModel(VaultService.LatestVaultVersion);
                else
                    CurrentViewModel = new ErrorViewModel("Cannot migrate vault.");
            }
        }

        private async Task ValidateVaultAsync(CancellationToken cancellationToken = default)
        {
            var result = await VaultService.VaultValidator.TryValidateAsync(_vaultModel.Folder, cancellationToken);
            if (result.Successful)
                return;

            CurrentViewModel = new ErrorViewModel(result.GetMessage());
        }

        private async Task TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var vaultLifecycle = await VaultManagerService.UnlockAsync(_vaultModel, _credentials, cancellationToken);
                VaultUnlocked?.Invoke(this, new(vaultLifecycle));
            }
            catch (Exception ex)
            {
                // If failed, restart the process
                // TODO: Above ^
                _ = ex;
            }
            finally
            {
                _credentials.Dispose();
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
                await ValidateVaultAsync();
        }

        private async void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is AuthenticationChangedEventArgs authArgs)
            {
                // Add authentication
                _credentials.Add(authArgs.Authentication);

                if (!await TryNextAuthAsync() && CurrentViewModel is not ErrorViewModel)
                {
                    // Reached the end, in which case we should try to unlock the vault
                    await TryUnlockAsync();
                }
            }
        }

        partial void OnCurrentViewModelChanging(ReportableViewModel? oldValue, ReportableViewModel? newValue)
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
            _credentials.Dispose();
            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
        }
    }
}