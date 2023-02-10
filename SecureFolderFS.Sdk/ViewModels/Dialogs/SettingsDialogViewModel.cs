using CommunityToolkit.Mvvm.Messaging;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class SettingsDialogViewModel : DialogViewModel
    {
        public static SettingsDialogViewModel Instance { get; } = new();

        public IMessenger Messenger { get; }

        private SettingsDialogViewModel()
        {
            Messenger = new WeakReferenceMessenger();
        }
    }
}
