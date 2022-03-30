using SecureFolderFS.Backend.ViewModels.Pages.SettingsDialog;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class SettingsNavigationRequestedMessage : ValueMessage<BaseSettingsDialogPageViewModel>
    {
        public SettingsNavigationRequestedMessage(BaseSettingsDialogPageViewModel value)
            : base(value)
        {
        }
    }
}
