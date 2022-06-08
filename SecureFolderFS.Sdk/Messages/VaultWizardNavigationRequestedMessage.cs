using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultWizardNavigationRequestedMessage : ValueMessage<BaseVaultWizardPageViewModel>
    {
        public VaultWizardNavigationRequestedMessage(BaseVaultWizardPageViewModel value)
            : base(value)
        {
        }
    }
}
