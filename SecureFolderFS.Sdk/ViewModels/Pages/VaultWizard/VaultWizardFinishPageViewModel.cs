using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
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

            WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessageDeprecated(DialogViewModel.VaultViewModel));
        }
    }
}
