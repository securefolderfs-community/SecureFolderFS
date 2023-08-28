using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Login;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>]
    public sealed partial class LoginViewModel : ObservableObject, IDisposable, IAsyncInitialize
    {
        private readonly bool _enableMigration;
        private readonly IVaultModel _vaultModel;
        private IReadOnlyList<AuthenticationModel>? _authList;
        private int _authPosition;

        [ObservableProperty] private BaseLoginViewModel? _CurrentViewModel;

        public LoginViewModel(IVaultModel vaultModel, bool enableMigration)
        {
            ServiceProvider = Ioc.Default;
            _enableMigration = enableMigration;
            _vaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _authList = await VaultService.VaultAuthenticator.GetAuthenticationAsync(_vaultModel.Folder, cancellationToken).ToListAsync(cancellationToken);

            var validator = VaultService.GetVaultValidator();
            var validationResult = await validator.TryValidateAsync(_vaultModel.Folder, cancellationToken);

            if (!validationResult.Successful)
            {
                // Try to migrate the vault if not supported
                if (_enableMigration && validationResult.Exception is NotSupportedException)
                {

                }
                else
                    CurrentViewModel = new ErrorViewModel();
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
