using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultCreator.Routine;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class SetPasswordPageViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationRoutineStep7 _step7;

        public SetPasswordPageViewModel(IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this._step7 = step7;

            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<HandledCallback?>(PrimaryButtonClick);
            DialogViewModel.SecondaryButtonClickCommand = new RelayCommand<HandledCallback?>(SecondaryButtonClick);
        }

        private void PrimaryButtonClick(HandledCallback? e)
        {
            e?.Handle();

            Messenger.Send(new PasswordClearRequestedMessage());
        }

        private void SecondaryButtonClick(HandledCallback? e)
        {
            Messenger.Send(new PasswordClearRequestedMessage());
        }
    }
}
