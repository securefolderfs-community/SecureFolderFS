using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public abstract class BaseVaultWizardPageViewModel : ObservableObject, IDisposable
    {
        public VaultWizardDialogViewModel DialogViewModel { get; }

        public IMessenger Messenger { get; }

        public bool CanGoBack { get; protected init; }

        protected BaseVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
        {
            this.Messenger = messenger;
            this.DialogViewModel = dialogViewModel;
            this.CanGoBack = true;
        }

        public virtual void ReattachCommands() { }

        public virtual void Dispose() { }
    }
}
