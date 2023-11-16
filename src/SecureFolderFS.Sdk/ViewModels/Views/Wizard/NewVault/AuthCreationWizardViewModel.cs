﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>]
    public sealed partial class AuthCreationWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IModifiableFolder _vaultFolder;

        [ObservableProperty] private CipherViewModel? _ContentCipher;
        [ObservableProperty] private CipherViewModel? _FileNameCipher;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _ContentCiphers;
        [ObservableProperty] private ObservableCollection<CipherViewModel> _FileNameCiphers;

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

        public AuthCreationWizardViewModel(IModifiableFolder vaultFolder, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultFolder = vaultFolder;
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

            var password = PasswordGetter?.Invoke();
            if (password is null)
                return;

            var vaultOptions = new VaultOptions()
            {
                ContentCipherId = ContentCipher.Id,
                FileNameCipherId = FileNameCipher.Id,
                AuthenticationMethod = "TODO",
                Specialization = "TODO"
            };

            // Create the vault
            var superSecret = await VaultService.VaultCreator.CreateVaultAsync(_vaultFolder, new[] { password }, vaultOptions, cancellationToken);

            // Navigate
            await NavigationService.TryNavigateAsync(() => new RecoveryKeyWizardViewModel(_vaultFolder, superSecret, DialogViewModel));
        }
    }
}
