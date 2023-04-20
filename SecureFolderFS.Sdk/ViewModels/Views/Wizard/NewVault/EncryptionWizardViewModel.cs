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

        [ObservableProperty] private CipherInfoViewModel? _ContentCipherItemViewModel;
        [ObservableProperty] private CipherInfoViewModel? _FileNameCipherItemViewModel;

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

            ArgumentNullException.ThrowIfNull(ContentCipherItemViewModel);
            ArgumentNullException.ThrowIfNull(FileNameCipherItemViewModel);

            if (!await _vaultCreationModel.SetCipherSchemeAsync(ContentCipherItemViewModel.Id, FileNameCipherItemViewModel.Id, cancellationToken))
                return; // TODO: Report issue

            var deployResult = await _vaultCreationModel.DeployAsync(cancellationToken);
            if (!deployResult.Successful)
                return; // TODO: Report issue

            // TODO: Handle adding vault to VaultCollectionModel here...

            WeakReferenceMessenger.Default.Send(new AddVaultMessage(deployResult.Value!));
            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(deployResult.Value!.VaultName, DialogViewModel));
        }
    }
}
