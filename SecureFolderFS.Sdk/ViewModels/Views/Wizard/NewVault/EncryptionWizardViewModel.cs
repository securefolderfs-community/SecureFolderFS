using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    public sealed partial class EncryptionWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;

        [ObservableProperty] private CipherInfoViewModel? _ContentCipher;
        [ObservableProperty] private CipherInfoViewModel? _FileNameCipher;
        [ObservableProperty] private ObservableCollection<CipherInfoViewModel> _ContentCiphers;
        [ObservableProperty] private ObservableCollection<CipherInfoViewModel> _FileNameCiphers;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public EncryptionWizardViewModel(IVaultCreationModel vaultCreationModel, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            _vaultCreationModel = vaultCreationModel;
            _ContentCiphers = new();
            _FileNameCiphers = new();

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.SecondaryButtonText = null; // Don't show the option to cancel the dialog
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in VaultService.GetContentCiphers())
            {
                var name = string.IsNullOrEmpty(item) ? "None" : item;
                ContentCiphers.Add(new(name));
            }

            foreach (var item in VaultService.GetFileNameCiphers())
            {
                var name = string.IsNullOrEmpty(item) ? "None" : item;
                FileNameCiphers.Add(new(item, name));
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            ArgumentNullException.ThrowIfNull(ContentCipher);
            ArgumentNullException.ThrowIfNull(FileNameCipher);

            if (!await _vaultCreationModel.SetCipherSchemeAsync(ContentCipher.Id, FileNameCipher.Id, cancellationToken))
                return; // TODO: Report issue

            var deployResult = await _vaultCreationModel.DeployAsync(cancellationToken);
            if (!deployResult.Successful || deployResult.Value is null)
                return; // TODO: Report issue

            // Add vault
            DialogViewModel.VaultCollectionModel.Add(deployResult.Value);
            await DialogViewModel.VaultCollectionModel.TrySaveAsync(cancellationToken);

            // Navigate
            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(deployResult.Value!.VaultName, DialogViewModel));
        }
    }
}
