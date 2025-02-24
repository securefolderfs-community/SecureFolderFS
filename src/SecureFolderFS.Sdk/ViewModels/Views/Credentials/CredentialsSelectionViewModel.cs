using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Inject<IVaultService>, Inject<IVaultCredentialsService>]
    [Bindable(true)]
    public sealed partial class CredentialsSelectionViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationType _authenticationStage;

        [ObservableProperty] private bool _CanRemoveCredentials;
        [ObservableProperty] private RegisterViewModel? _RegisterViewModel;
        [ObservableProperty] private AuthenticationViewModel? _ConfiguredViewModel;
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions;

        public IDisposable? UnlockContract { private get; set; }

        public event EventHandler<CredentialsConfirmationViewModel>? ConfirmationRequested;

        public CredentialsSelectionViewModel(IFolder vaultFolder, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _authenticationStage = authenticationStage;
            _CanRemoveCredentials = authenticationStage != AuthenticationType.FirstStageOnly;
            _AuthenticationOptions = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get authentication options
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            if (vaultOptions.VaultId is null)
                return;

            await foreach (var item in VaultCredentialsService.GetCreationAsync(_vaultFolder, vaultOptions.VaultId, cancellationToken))
            {
                // Don't add authentication methods to list which are already in use
                if (vaultOptions.AuthenticationMethod.Contains(item.Id))
                    continue;

                // Don't add authentication methods which are not allowed in the Authentication Stage
                if (!item.Availability.HasFlag(_authenticationStage))
                    continue;

                AuthenticationOptions.Add(item);
            }
        }

        [RelayCommand]
        private void RemoveCredentials()
        {
            if (ConfiguredViewModel is null || RegisterViewModel is null || UnlockContract is null)
                return;

            ConfirmationRequested?.Invoke(this, new(_vaultFolder, RegisterViewModel, _authenticationStage)
            {
                IsRemoving = true,
                CanComplement = false,
                UnlockContract = UnlockContract,
                ConfiguredViewModel = ConfiguredViewModel
            });
        }

        [RelayCommand]
        private async Task ItemSelected(AuthenticationViewModel? authenticationViewModel, CancellationToken cancellationToken)
        {
            // We need to get the current authentication equivalent in 'creation' instead of 'login' mode
            authenticationViewModel ??= await GetExistingCreationForLoginAsync(cancellationToken);
            if (authenticationViewModel is null)
                return;
            
            if (UnlockContract is null || RegisterViewModel is null)
                return;

            RegisterViewModel.CurrentViewModel = authenticationViewModel;
            ConfirmationRequested?.Invoke(this, new(_vaultFolder, RegisterViewModel, _authenticationStage)
            {
                IsRemoving = false,
                CanComplement = _authenticationStage != AuthenticationType.FirstStageOnly, // TODO: Also add a flag to the AuthenticationViewModel to indicate if it can be complemented
                UnlockContract = UnlockContract,
                ConfiguredViewModel = ConfiguredViewModel
            });
        }

        private async Task<AuthenticationViewModel?> GetExistingCreationForLoginAsync(CancellationToken cancellationToken)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            if (vaultOptions.VaultId is null)
                return null;

            return await VaultCredentialsService.GetCreationAsync(_vaultFolder, vaultOptions.VaultId, cancellationToken)
                .FirstOrDefaultAsync(x => x.Id == ConfiguredViewModel?.Id, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ConfirmationRequested = null;
            RegisterViewModel?.Dispose();
            ConfiguredViewModel?.Dispose();
            AuthenticationOptions.DisposeElements();
        }
    }
}
