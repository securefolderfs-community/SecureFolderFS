using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.ViewModels.Dialogs;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public abstract class BaseVaultWizardPageViewModel : ObservableObject
    {
        protected IMessenger Messenger { get; }

        protected VaultWizardDialogViewModel DialogViewModel { get; }

        protected BaseVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
        {
            this.Messenger = messenger;
            this.DialogViewModel = dialogViewModel;
        }

        public virtual void UpdateViewModelOnReturn() { }
    }
}
