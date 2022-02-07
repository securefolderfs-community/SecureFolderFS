using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class AddExistingVaultPageViewModel : BaseVaultWizardPageViewModel
    {
        public AddExistingVaultPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            dialogViewModel.Title = "Add existing vault";
        }
    }
}
