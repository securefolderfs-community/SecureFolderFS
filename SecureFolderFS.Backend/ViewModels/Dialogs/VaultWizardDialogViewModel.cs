using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class VaultWizardDialogViewModel : ObservableObject
    {
        public IMessenger Messenger { get; }

        public VaultViewModel? VaultViewModel { get; set; }

        private bool _IsPrimaryButtonEnabled;
        public bool IsPrimaryButtonEnabled
        {
            get => _IsPrimaryButtonEnabled;
            set => SetProperty(ref _IsPrimaryButtonEnabled, value);
        }

        public IRelayCommand? PrimaryButtonClickCommand { get; set; }

        public IRelayCommand? SecondaryButtonClickCommand { get; set; }

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
