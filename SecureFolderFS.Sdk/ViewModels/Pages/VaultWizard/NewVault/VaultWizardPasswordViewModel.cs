using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault
{
    public sealed class VaultWizardPasswordViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;

        public Func<IPassword>? InitializeWithPassword { get; set; }

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        public VaultWizardPasswordViewModel(IVaultCreationModel vaultCreationModel, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _vaultCreationModel = vaultCreationModel;

            // Always false since passwords are not preserved
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken)
        {
            flag?.NoForwarding();

            if (!await _vaultCreationModel.SetPasswordAsync(InitializeWithPassword!(), cancellationToken))
                return; // TODO: Report issue

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardEncryptionViewModel(_vaultCreationModel, Messenger, DialogViewModel)));
        }
    }
}
