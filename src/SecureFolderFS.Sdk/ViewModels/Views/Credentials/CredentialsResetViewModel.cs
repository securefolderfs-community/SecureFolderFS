using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Inject<IVaultManagerService>, Inject<IVaultService>, Inject<IVaultCredentialsService>]
    [Bindable(true)]
    public sealed partial class CredentialsResetViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly IDisposable _unlockContract;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions = new();

        public CredentialsResetViewModel(IFolder vaultFolder, IDisposable unlockContract, RegisterViewModel registerViewModel)
        {
            ServiceProvider = DI.Default;
            RegisterViewModel = registerViewModel;
            _vaultFolder = vaultFolder;
            _unlockContract = unlockContract;
            _credentialsTcs = new();

            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            if (vaultOptions.VaultId is null)
                return;

            AuthenticationOptions.Clear();
            await foreach (var item in VaultCredentialsService.GetCreationAsync(_vaultFolder, vaultOptions.VaultId, cancellationToken))
                AuthenticationOptions.Add(item);

            RegisterViewModel.CurrentViewModel = AuthenticationOptions.FirstOrDefault();
        }

        [RelayCommand]
        public async Task ConfirmAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);

            var key = await _credentialsTcs.Task;
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var newOptions = configuredOptions with
            {
                UnlockProcedure = new AuthenticationMethod([ RegisterViewModel.CurrentViewModel.Id ], null)
            };

            await VaultManagerService.ModifyAuthenticationAsync(_vaultFolder, _unlockContract, key, newOptions, cancellationToken);
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RegisterViewModel.CredentialsProvided -= RegisterViewModel_CredentialsProvided;
        }
    }
}
