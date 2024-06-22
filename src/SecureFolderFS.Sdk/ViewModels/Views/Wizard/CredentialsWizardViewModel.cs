using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
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
        private readonly KeyChain _credentials;
        private readonly string _vaultId;

        [ObservableProperty] private CipherViewModel? _ContentCipher;
        [ObservableProperty] private CipherViewModel? _FileNameCipher;
        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _ContentCiphers = new();
        [ObservableProperty] private ObservableCollection<CipherViewModel> _FileNameCiphers = new();
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions = new();

        public IModifiableFolder Folder { get; }

        public CredentialsWizardViewModel(IModifiableFolder folder)
        {
            ServiceProvider = Ioc.Default;
            Title = "SetPassword".ToLocalized();
            CanContinue = false;
            CanCancel = true;
            Folder = folder;
            _credentials = new();
            _vaultId = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public override async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ContentCipher);
            ArgumentNullException.ThrowIfNull(FileNameCipher);
            ArgumentNullException.ThrowIfNull(CurrentViewModel);

            // Make sure to also dispose the data within the current view model whether the navigation is successful or not
            using (CurrentViewModel)
            {
                var vaultOptions = new VaultOptions()
                {
                    ContentCipherId = ContentCipher.Id,
                    FileNameCipherId = FileNameCipher.Id,
                    AuthenticationMethod = CurrentViewModel.Id,
                    VaultId = _vaultId
                };

                // In case the authentication was not reported, try to extract it manually, if possible
                if (_credentials.Count == 0 && CurrentViewModel is IWrapper<IKey> keyWrapper)
                    _credentials.Push(keyWrapper.Inner);

                // Create the vault
                var superSecret = await VaultManagerService.CreateAsync(
                    Folder,
                    _credentials,
                    vaultOptions,
                    cancellationToken);

                return new CredentialsResult(superSecret, _vaultId);
            }
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Failure(null));
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
            await foreach (var item in VaultService.GetAllSecurityAsync(Folder, _vaultId))
                AuthenticationOptions.Add(item);

            // Set default authentication option
            CurrentViewModel = AuthenticationOptions.FirstOrDefault();

            static void EnumerateCiphers(IEnumerable<string> source, ICollection<CipherViewModel> destination)
            {
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
            _credentials.Dispose();
            CurrentViewModel?.Dispose();

            if (CurrentViewModel is not null)
            {
                CurrentViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
            }
        }

        partial void OnCurrentViewModelChanged(AuthenticationViewModel? oldValue, AuthenticationViewModel? newValue)
        {
            // Make sure to dispose the old value in case there was authentication data provided
            oldValue?.Dispose();

            if (oldValue is not null)
            {
                oldValue.StateChanged -= CurrentViewModel_StateChanged;
                oldValue.CredentialsProvided -= CurrentViewModel_CredentialsProvided;
            }

            if (newValue is not null)
            {
                newValue.StateChanged += CurrentViewModel_StateChanged;
                newValue.CredentialsProvided += CurrentViewModel_CredentialsProvided;
            }

            CanContinue = false;
        }

        private void CurrentViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentials.Push(e.Authentication);
            CanContinue = true;
        }

        private void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is PasswordChangedEventArgs args)
                CanContinue = args.IsMatch;
        }
    }
}
