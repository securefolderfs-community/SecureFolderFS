using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class MainVaultWizardPageViewModel : BaseVaultWizardPageViewModel
    {
        public IRelayCommand AddExistingVaultCommand { get; }

        public IRelayCommand CreateNewVaultCommand { get; }

        public MainVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            AddExistingVaultCommand = new RelayCommand(AddExistingVault);
            CreateNewVaultCommand = new RelayCommand(CreateNewVault);
        }

        private void AddExistingVault()
        {
            Messenger.Send(new NavigationRequestedMessage(new VaultWizardAddExistingViewModel(Messenger, DialogViewModel)));
        }

        private void CreateNewVault()
        {
            Messenger.Send(new NavigationRequestedMessage(new VaultWizardCreationPathViewModel(Messenger, DialogViewModel)));
        }
    }
}
