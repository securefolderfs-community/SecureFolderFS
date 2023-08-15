using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Login;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IThreadingService>, Inject<IVaultService>]
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly ICredentialsBuilder _credentialsBuilder;
        private readonly IAsyncValidator<IFolder> _vaultValidator;
        private IAsyncEnumerator<AuthenticationModel>? _enumerator;

        [ObservableProperty] private INotifyPropertyChanged? _LoginTypeViewModel;
        [ObservableProperty] private string? _VaultName;

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
            : base(vaultViewModel, navigationService)
        {
            ServiceProvider = Ioc.Default;
            VaultName = vaultViewModel.VaultModel.VaultName;
            _credentialsBuilder = VaultService.VaultUnlocker.GetCredentialsBuilder();
            _vaultValidator = VaultService.GetVaultValidator();

            _vaultWatcherModel = new VaultWatcherModel(vaultViewModel.VaultModel.Folder);
            _vaultWatcherModel.StateChanged += VaultWatcherModel_StateChanged;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize vault watcher
            await _vaultWatcherModel.InitAsync(cancellationToken);

            // Get the authentication method enumerator for this vault
            _enumerator = VaultService.VaultAuthenticator.GetAuthenticationAsync(VaultViewModel.VaultModel.Folder, cancellationToken).GetAsyncEnumerator(cancellationToken);

            // Set up the first authentication method
            if (!await TryNextAuthAsync())
                LoginTypeViewModel = new ErrorViewModel("No authentication methods available");
        }

        private async Task<bool> TryNextAuthAsync()
        {
            if (_enumerator is null || !await _enumerator.MoveNextAsync())
                return false;

            // Get the appropriate method
            var authenticationModel = _enumerator.Current;
            LoginTypeViewModel = authenticationModel.AuthenticationType switch
            {
                AuthenticationType.Password => new PasswordViewModel(authenticationModel),
                AuthenticationType.Other => new AuthenticationViewModel(authenticationModel),
                _ => new ErrorViewModel("Could not determine the authentication type.")
            };

            return true;
        }

        private async void LoginViewModel_StateChanged(object? sender, EventArgs e)
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

        private async void VaultWatcherModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is VaultChangedEventArgs { ContentsChanged: true })
                await ValidateVaultAsync();
        }

        private async Task ValidateVaultAsync(CancellationToken cancellationToken = default)
        {
            var result = await _vaultValidator.TryValidateAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
            if (result.Successful)
                return;

            LoginTypeViewModel = new ErrorViewModel(result.GetMessage());
        }

        private async Task TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var credentials = _credentialsBuilder.BuildCredentials();
                var lifetimeModel = await VaultService.VaultUnlocker.UnlockAsync(VaultViewModel.VaultModel, credentials, cancellationToken);

                // Update last access date
                await VaultViewModel.VaultModel.SetLastAccessDateAsync(DateTime.Now, cancellationToken);

                // Create view models
                var unlockedVaultViewModel = new UnlockedVaultViewModel(VaultViewModel, lifetimeModel);
                var dashboardPage = new VaultDashboardPageViewModel(unlockedVaultViewModel, NavigationService);

                // Notify that the vault has been unlocked
                WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultViewModel.VaultModel));

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

        partial void OnLoginTypeViewModelChanging(INotifyPropertyChanged? oldValue, INotifyPropertyChanged? newValue)
        {
            // Unhook old
            if (oldValue is INotifyStateChanged notifyStateChanged)
                notifyStateChanged.StateChanged -= LoginViewModel_StateChanged;

            // Hook up new
            if (newValue is INotifyStateChanged notifyStateChanged2)
                notifyStateChanged2.StateChanged += LoginViewModel_StateChanged;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _credentialsBuilder.Dispose();
            _vaultWatcherModel.Dispose();
            _vaultWatcherModel.StateChanged -= VaultWatcherModel_StateChanged;

            if (LoginTypeViewModel is INotifyStateChanged notifyStateChanged)
                notifyStateChanged.StateChanged -= LoginViewModel_StateChanged;
        }
    }
}
