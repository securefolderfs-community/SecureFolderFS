using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>, Inject<IVaultManagerService>]
    public sealed partial class AuthCreationWizardViewModel : BaseWizardPageViewModel
    {
        private readonly string _vaultId;
        private readonly IModifiableFolder _vaultFolder;

        [ObservableProperty] private CipherViewModel? _ContentCipher;
        [ObservableProperty] private CipherViewModel? _FileNameCipher;
        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _ContentCiphers;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _FileNameCiphers;
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions;

        public AuthCreationWizardViewModel(IModifiableFolder vaultFolder, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultId = Guid.NewGuid().ToString();
            _vaultFolder = vaultFolder;
            _ContentCiphers = new();
            _FileNameCiphers = new();
            _AuthenticationOptions = new();

            // Disallow continuation before passwords are validated
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get ciphers
            EnumerateCiphers(VaultService.GetContentCiphers(), ContentCiphers);
            EnumerateCiphers(VaultService.GetFileNameCiphers(), FileNameCiphers);

            // Set default cipher options
            ContentCipher = ContentCiphers.FirstOrDefault();
            FileNameCipher = FileNameCiphers.FirstOrDefault();

            // Get authentication options
            await foreach (var item in VaultManagerService.GetCreationAuthenticationAsync(_vaultFolder, _vaultId, cancellationToken))
                AuthenticationOptions.Add(item);

            // Set default authentication option
            CurrentViewModel = AuthenticationOptions.FirstOrDefault();
            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged += CurrentViewModel_StateChanged;

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
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            ArgumentNullException.ThrowIfNull(ContentCipher);
            ArgumentNullException.ThrowIfNull(FileNameCipher);
            ArgumentNullException.ThrowIfNull(CurrentViewModel);

            // Make sure to also dispose the data within the current view model whether the navigation is successful or not
            using (CurrentViewModel)
            {
                using var key = CurrentViewModel.RetrieveKey();
                if (key is null)
                    return;

                var vaultOptions = new VaultOptions()
                {
                    ContentCipherId = ContentCipher.Id,
                    FileNameCipherId = FileNameCipher.Id,
                    AuthenticationMethod = CurrentViewModel.Id,
                    VaultId = _vaultId
                };

                // Create the vault
                var superSecret = await VaultManagerService.CreateVaultAsync(
                    _vaultFolder,
                    new[] { key },
                    vaultOptions,
                    cancellationToken);

                // Navigate
                await NavigationService.TryNavigateAsync(() => new RecoveryKeyWizardViewModel(_vaultFolder, superSecret, DialogViewModel));
            }
        }

        public override void OnNavigatingFrom()
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
        }

        partial void OnCurrentViewModelChanged(AuthenticationViewModel? oldValue, AuthenticationViewModel? newValue)
        {
            // Make sure to dispose the old value in case there was authentication data provided
            oldValue?.Dispose();

            if (oldValue is not null)
                oldValue.StateChanged -= CurrentViewModel_StateChanged;

            if (newValue is not null)
                newValue.StateChanged += CurrentViewModel_StateChanged;

            DialogViewModel.PrimaryButtonEnabled = false;
        }

        private void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is AuthenticationChangedEventArgs)
                DialogViewModel.PrimaryButtonEnabled = true;
        }
    }
}
