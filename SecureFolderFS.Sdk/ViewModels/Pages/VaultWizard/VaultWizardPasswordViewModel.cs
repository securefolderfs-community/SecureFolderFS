using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardPasswordViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationRoutineStep7 _step7;

        public Func<DisposablePassword>? InitializeWithPassword { get; set; }

        public VaultWizardPasswordViewModel(IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _step7 = step7;
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            var step9 = _step7.InitializeKeystoreData(InitializeWithPassword!()).ContinueKeystoreFileInitialization();

            NextViewModel = new VaultWizardEncryptionViewModel(step9, Messenger, DialogViewModel);

            Messenger.Send(new VaultWizardNavigationRequestedMessage(NextViewModel));

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _step7.Dispose();
        }
    }
}
