using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IVaultService>, Inject<IVaultManagerService>]
    [Bindable(true)]
    public sealed partial class CredentialsWizardViewModel : BaseWizardViewModel
    {
        private readonly string _vaultId;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private CipherViewModel? _ContentCipher;
        [ObservableProperty] private CipherViewModel? _FileNameCipher;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _ContentCiphers = new();
        [ObservableProperty] private ObservableCollection<CipherViewModel> _FileNameCiphers = new();
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions = new();
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;

        public IModifiableFolder Folder { get; }

        public CredentialsWizardViewModel(IModifiableFolder folder)
        {
            ServiceProvider = DI.Default;
            Folder = folder;
            _credentialsTcs = new();
            _RegisterViewModel = new();
            _vaultId = Guid.NewGuid().ToString();

            ContinueText = "Continue".ToLocalized();
            Title = "SetCredentials".ToLocalized();
            CancelText = "Cancel".ToLocalized();
            CanContinue = false;
            CanCancel = true;

            RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        /// <inheritdoc/>
        public override async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ContentCipher);
            ArgumentNullException.ThrowIfNull(FileNameCipher);
            ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);

            // Await the credentials
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);
            var credentials = await _credentialsTcs.Task;

            // Make sure to also dispose the data within the current view model whether the navigation is successful or not
            using (RegisterViewModel.CurrentViewModel)
            {
                var vaultOptions = new VaultOptions()
                {
                    ContentCipherId = ContentCipher.Id,
                    FileNameCipherId = FileNameCipher.Id,
                    AuthenticationMethod = [ RegisterViewModel.CurrentViewModel.Id ],
                    VaultId = _vaultId
                };

                // Create the vault
                var unlockContract = await VaultManagerService.CreateAsync(
                    Folder,
                    credentials,
                    vaultOptions,
                    cancellationToken);

                return new CredentialsResult(unlockContract, _vaultId);
            }
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            // Get ciphers
            EnumerateCiphers(VaultService.GetContentCiphers(), ContentCiphers);
            EnumerateCiphers(VaultService.GetFileNameCiphers(), FileNameCiphers);

            // Set default cipher options
            ContentCipher = ContentCiphers.FirstOrDefault();
            FileNameCipher = FileNameCiphers.FirstOrDefault();

            // Get authentication options
            AuthenticationOptions.Clear();
            await foreach (var item in VaultService.GetCreationAsync(Folder, _vaultId))
                AuthenticationOptions.Add(item);

            // Set default authentication option
            RegisterViewModel.CurrentViewModel = AuthenticationOptions.FirstOrDefault();
            return;

            static void EnumerateCiphers(IEnumerable<string> source, ICollection<CipherViewModel> destination)
            {
                destination.Clear();
                foreach (var item in source)
                {
                    var name = string.IsNullOrEmpty(item) ? "NoEncryption".ToLocalized() : item;
                    destination.Add(new(item, name));
                }
            }
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            RegisterViewModel.PropertyChanged -= RegisterViewModel_PropertyChanged;
            RegisterViewModel.CredentialsProvided -= RegisterViewModel_CredentialsProvided;
            AuthenticationOptions.DisposeElements();
            RegisterViewModel.Dispose();
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }

        private void RegisterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegisterViewModel.CanContinue))
                CanContinue = RegisterViewModel.CanContinue;
        }
    }
}
