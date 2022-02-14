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

        private string? _Title = "Add new vault";
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private bool _IsConfirmButtonVisible;
        public bool IsConfirmButtonVisible
        {
            get => _IsConfirmButtonVisible;
            set => SetProperty(ref _IsConfirmButtonVisible, value);
        }

        private bool _IsConfirmButtonEnabled;
        public bool IsConfirmButtonEnabled
        {
            get => _IsConfirmButtonEnabled;
            set => SetProperty(ref _IsConfirmButtonEnabled, value);
        }

        private IRelayCommand? _ConfirmCommand;
        public IRelayCommand? ConfirmCommand
        {
            get => _ConfirmCommand;
            set => SetProperty(ref _ConfirmCommand, value);
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
