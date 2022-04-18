using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.VaultCreator.Routine;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class SetPasswordPageViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationRoutineStep7 _step7;

        private ChooseEncryptionPageViewModel? _nextViewModel;

        public Func<DisposablePassword>? InitializeWithPassword { get; set; }

        public SetPasswordPageViewModel(IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this._step7 = step7;

            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(PrimaryButtonClick);
            DialogViewModel.SecondaryButtonClickCommand = new RelayCommand<IHandledFlag?>(SecondaryButtonClick);
        }

        public override void ReattachCommands()
        {
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(PrimaryButtonClick);
        }

        private void PrimaryButtonClick(IHandledFlag? e)
        {
            e?.Handle();

            var step9 = _step7.InitializeKeystoreData(InitializeWithPassword!())
                .ContinueKeystoreFileInitialization();

            _nextViewModel = new(step9, Messenger, DialogViewModel);

            Messenger.Send(new VaultWizardNavigationRequestedMessage(_nextViewModel));
        }

        private void SecondaryButtonClick(IHandledFlag? e)
        {
            Messenger.Send(new PasswordClearRequestedMessage());
        }

        public override void Dispose()
        {
            _step7.Dispose();
        }
    }
}
