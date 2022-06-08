using SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class SettingsNavigationRequestedMessage : ValueMessage<BaseSettingsDialogPageViewModel>
    {
        public SettingsNavigationRequestedMessage(BaseSettingsDialogPageViewModel value)
            : base(value)
        {
        }
    }
}
