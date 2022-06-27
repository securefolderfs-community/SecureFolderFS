using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardSummaryViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardSummaryViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            CanGoBack = false;
            DialogViewModel.PrimaryButtonEnabled = true;
            VaultName = DialogViewModel.VaultViewModel!.VaultName;

            WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessageDeprecated(DialogViewModel.VaultViewModel));
        }
    }
}
