using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>]
    public sealed partial class PasswordWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreator _vaultCreator;
        private readonly IModifiableFolder _vaultFolder;

        [ObservableProperty] private CipherInfoViewModel? _ContentCipher;
        [ObservableProperty] private CipherInfoViewModel? _FileNameCipher;
        [ObservableProperty] private ObservableCollection<CipherInfoViewModel> _ContentCiphers;
        [ObservableProperty] private ObservableCollection<CipherInfoViewModel> _FileNameCiphers;

        /// <summary>
        /// Gets or sets the password getter delegate used to retrieve the password from the view.
        /// </summary>
        public Func<IPassword?>? PasswordGetter { get; set; }

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        public PasswordWizardViewModel(IModifiableFolder vaultFolder, IVaultCreator vaultCreator, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultFolder = vaultFolder;
            _vaultCreator = vaultCreator;
            _ContentCiphers = new();
            _FileNameCiphers = new();

            // Disallow continuation before passwords are validated
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            EnumerateCiphers(VaultService.GetContentCiphers(), ContentCiphers);
            EnumerateCiphers(VaultService.GetFileNameCiphers(), FileNameCiphers);

            ContentCipher = ContentCiphers.FirstOrDefault();
            FileNameCipher = FileNameCiphers.FirstOrDefault();

            return Task.CompletedTask;

            static void EnumerateCiphers(IEnumerable<string> source, ICollection<CipherInfoViewModel> destination)
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

            var password = PasswordGetter?.Invoke();
            if (password is null)
                return;

            // Create the vault
            var superSecret = await _vaultCreator.CreateVaultAsync(_vaultFolder, password, FileNameCipher.Id, ContentCipher.Id, cancellationToken);

            // Navigate
            await NavigationService.TryNavigateAsync(() => new RecoveryKeyWizardViewModel(_vaultFolder, superSecret, DialogViewModel));
        }
    }
}
