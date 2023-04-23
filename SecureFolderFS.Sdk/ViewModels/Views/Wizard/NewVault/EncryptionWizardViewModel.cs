using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    public sealed partial class EncryptionWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;

        [ObservableProperty] private CipherInfoViewModel? _ContentCipherViewModel;
        [ObservableProperty] private CipherInfoViewModel? _FileNameCipherViewModel;

        public EncryptionWizardViewModel(IVaultCreationModel vaultCreationModel, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            _vaultCreationModel = vaultCreationModel;

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.SecondaryButtonText = null; // Don't show the option to cancel the dialog
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            ArgumentNullException.ThrowIfNull(ContentCipherViewModel);
            ArgumentNullException.ThrowIfNull(FileNameCipherViewModel);

            if (!await _vaultCreationModel.SetCipherSchemeAsync(ContentCipherViewModel.Id, FileNameCipherViewModel.Id, cancellationToken))
                return; // TODO: Report issue

            var deployResult = await _vaultCreationModel.DeployAsync(cancellationToken);
            if (!deployResult.Successful)
                return; // TODO: Report issue

            // Add vault
            DialogViewModel.VaultCollectionModel.AddVault(deployResult.Value!);
            await DialogViewModel.VaultCollectionModel.SaveAsync(cancellationToken);
            WeakReferenceMessenger.Default.Send(new AddVaultMessage(deployResult.Value!));

            // Navigate
            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(deployResult.Value!.VaultName, DialogViewModel));
        }
    }
}
