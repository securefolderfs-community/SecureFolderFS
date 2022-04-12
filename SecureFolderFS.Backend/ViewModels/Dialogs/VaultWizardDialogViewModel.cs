using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;
using System.Windows.Input;

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class VaultWizardDialogViewModel : BaseDialogViewModel
    {
        public IMessenger Messenger { get; }

        public VaultViewModel? VaultViewModel { get; set; }

        public new ICommand? PrimaryButtonClickCommand { get; set; }

        public new ICommand? SecondaryButtonClickCommand { get; set; }

        public VaultWizardDialogViewModel()
        {
            this.Messenger = new WeakReferenceMessenger();
        }

        public void StartNavigation()
        {
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardMainPageViewModel(Messenger, this)));
        }
    }
}
