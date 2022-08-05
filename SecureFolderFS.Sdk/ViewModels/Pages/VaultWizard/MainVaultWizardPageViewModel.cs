using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed partial class MainVaultWizardPageViewModel : BaseVaultWizardPageViewModel
    {
        public MainVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
        }

        [RelayCommand]
        private void AddExistingVault()
        {
            Messenger.Send(new NavigationRequestedMessage(new VaultWizardSelectPathViewModel(Messenger, DialogViewModel)));
        }

        [RelayCommand]
        private void CreateNewVault()
        {
            Messenger.Send(new NavigationRequestedMessage(new VaultWizardCreationPathViewModel(Messenger, DialogViewModel)));
        }
    }
}
