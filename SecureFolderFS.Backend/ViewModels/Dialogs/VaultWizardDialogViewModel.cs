using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class VaultWizardDialogViewModel : ObservableObject
    {
        public IMessenger Messenger { get; }

        public VaultModel? VaultModel { get; set; }

        private string? _Title = "Add new vault";
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        public VaultWizardDialogViewModel()
        {
            this.Messenger = new WeakReferenceMessenger();
        }

        public void StartNavigation()
        {
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardMainPageViewModel(Messenger, this)));
        }
    }
}
