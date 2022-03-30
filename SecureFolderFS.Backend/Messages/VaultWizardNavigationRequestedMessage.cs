using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultWizardNavigationRequestedMessage : ValueMessage<BaseVaultWizardPageViewModel>
    {
        public VaultWizardNavigationRequestedMessage(BaseVaultWizardPageViewModel value)
            : base(value)
        {
        }
    }
}
