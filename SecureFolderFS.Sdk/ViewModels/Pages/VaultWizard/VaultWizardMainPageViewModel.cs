using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardMainPageViewModel : BaseVaultWizardPageViewModel
    {
        public IRelayCommand AddExistingVaultCommand { get; }

        public IRelayCommand CreateNewVaultCommand { get; }

        public VaultWizardMainPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            AddExistingVaultCommand = new RelayCommand(AddExistingVault);
            CreateNewVaultCommand = new RelayCommand(CreateNewVault);
        }

        private void AddExistingVault()
        {
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new AddExistingVaultPageViewModel(Messenger, DialogViewModel)));
        }

        private void CreateNewVault()
        {
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new ChooseVaultCreationPathPageViewModel(Messenger, DialogViewModel)));
        }
    }
}
