using CommunityToolkit.Mvvm.Messaging;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class SettingsDialogViewModel : BaseDialogViewModel
    {
        public IMessenger Messenger { get; }

        public SettingsDialogViewModel()
        {
            Messenger = new WeakReferenceMessenger();
        }
    }
}
