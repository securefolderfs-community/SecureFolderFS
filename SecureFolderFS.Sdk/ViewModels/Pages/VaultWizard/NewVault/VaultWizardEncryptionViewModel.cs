using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault
{
    public sealed partial class VaultWizardEncryptionViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;

        [ObservableProperty]
        private CipherItemViewModel? _ContentCipherItemViewModel;

        [ObservableProperty]
        private CipherItemViewModel? _FileNameCipherItemViewModel;

        public VaultWizardEncryptionViewModel(IVaultCreationModel vaultCreationModel, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _vaultCreationModel = vaultCreationModel;

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.SecondaryButtonText = null; // Don't show the option to cancel the dialog
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken)
        {
            flag?.NoForwarding();

            ArgumentNullException.ThrowIfNull(ContentCipherItemViewModel);
            ArgumentNullException.ThrowIfNull(FileNameCipherItemViewModel);

            if (!await _vaultCreationModel.SetCipherSchemeAsync(ContentCipherItemViewModel.CipherInfoModel, FileNameCipherItemViewModel.CipherInfoModel, cancellationToken))
                return; // TODO: Report issue

            var deployResult = await _vaultCreationModel.DeployAsync(cancellationToken);
            if (!deployResult.IsSuccess || deployResult.Value is null)
                return; // TODO: Report issue

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardSummaryViewModel(deployResult.Value, Messenger, DialogViewModel)));
        }
    }
}
