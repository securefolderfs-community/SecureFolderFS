using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>, Inject<IVaultService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationStage _authenticationStage;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _CanComplement;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private AuthenticationViewModel? _ConfiguredViewModel;

        public required IDisposable UnlockContract { private get; init; }

        public CredentialsConfirmationViewModel(IFolder vaultFolder, RegisterViewModel registerViewModel, AuthenticationStage authenticationStage)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _RegisterViewModel = registerViewModel;
            _authenticationStage = authenticationStage;
            _credentialsTcs = new();

            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        [RelayCommand]
        public async Task ConfirmAsync(CancellationToken cancellationToken)
        {
            if (IsRemoving)
                await RemoveAsync(cancellationToken);
            else
                await ModifyAsync(cancellationToken);
        }

        private async Task ModifyAsync(CancellationToken cancellationToken)
        {
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);
            var key = await _credentialsTcs.Task;
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var authenticationMethod = GetAuthenticationMethod();

            await ChangeCredentialsAsync(key, configuredOptions, authenticationMethod, cancellationToken);
            return;

            string[] GetAuthenticationMethod()
            {
                ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);
                return _authenticationStage switch
                {
                    AuthenticationStage.ProceedingStageOnly => [ configuredOptions.AuthenticationMethod[0], RegisterViewModel.CurrentViewModel.Id ],
                    AuthenticationStage.FirstStageOnly => configuredOptions.AuthenticationMethod.Length > 1
                        ? [ RegisterViewModel.CurrentViewModel.Id, configuredOptions.AuthenticationMethod[1] ]
                        : [ RegisterViewModel.CurrentViewModel.Id ],

                    _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
                };
            }
        }

        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            if (_authenticationStage != AuthenticationStage.ProceedingStageOnly)
                return;

            var key = RegisterViewModel.Credentials.Keys.First();
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var authenticationMethod = new[] { configuredOptions.AuthenticationMethod[0] };

            await ChangeCredentialsAsync(key, configuredOptions, authenticationMethod, cancellationToken);
        }

        private async Task ChangeCredentialsAsync(IKey key, VaultOptions configuredOptions, string[] authenticationMethod, CancellationToken cancellationToken)
        {
            var newOptions = new VaultOptions()
            {
                AuthenticationMethod = authenticationMethod,
                ContentCipherId = configuredOptions.ContentCipherId,
                FileNameCipherId = configuredOptions.FileNameCipherId,
                NameEncodingId = configuredOptions.NameEncodingId,
                VaultId = configuredOptions.VaultId,
                Version = configuredOptions.Version
            };

            await VaultManagerService.ModifyAuthenticationAsync(_vaultFolder, UnlockContract, key, newOptions, cancellationToken);
            if (ConfiguredViewModel is not null)
                await ConfiguredViewModel.RevokeAsync(null, cancellationToken);
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
