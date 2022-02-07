using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultWizardNavigationRequestedMessage : ValueChangedMessage<BaseVaultWizardPageViewModel>
    {
        public VaultWizardNavigationRequestedMessage(BaseVaultWizardPageViewModel value)
            : base(value)
        {
        }
    }
}
