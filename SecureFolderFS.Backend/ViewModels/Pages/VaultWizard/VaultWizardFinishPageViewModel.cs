using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardFinishPageViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardFinishPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            CanGoBack = false;
            DialogViewModel.PrimaryButtonEnabled = true;
            VaultName = DialogViewModel.VaultViewModel!.VaultName;

            WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessage(DialogViewModel.VaultViewModel));
        }
    }
}
