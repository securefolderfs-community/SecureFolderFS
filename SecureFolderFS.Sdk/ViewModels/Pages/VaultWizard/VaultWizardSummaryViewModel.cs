using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardSummaryViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardSummaryViewModel(IVaultModel vaultModel, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = true;
            VaultName = vaultModel.VaultName;

            WeakReferenceMessenger.Default.Send(new AddVaultMessage(vaultModel));
        }
    }
}
