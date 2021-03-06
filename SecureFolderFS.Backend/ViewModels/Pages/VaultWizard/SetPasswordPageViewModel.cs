using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.VaultCreator.Routine;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class SetPasswordPageViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationRoutineStep7 _step7;

        public Func<DisposablePassword>? InitializeWithPassword { get; set; }

        public SetPasswordPageViewModel(IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this._step7 = step7;
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            var step9 = _step7.InitializeKeystoreData(InitializeWithPassword!()).ContinueKeystoreFileInitialization();

            NextViewModel = new ChooseEncryptionPageViewModel(step9, Messenger, DialogViewModel);

            Messenger.Send(new VaultWizardNavigationRequestedMessage(NextViewModel));

            return Task.CompletedTask;
        }

        public override Task SecondaryButtonClick(IEventDispatchFlag? flag)
        {
            Messenger.Send(new PasswordClearRequestedMessage());
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _step7.Dispose();
        }
    }
}
