using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardSummaryViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardSummaryViewModel(IFolder vaultFolder, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = true;

            var vaultModel = new LocalVaultModel(vaultFolder);
            VaultName = vaultModel.VaultName;
            WeakReferenceMessenger.Default.Send(new AddVaultMessage(vaultModel));
        }
    }
}
