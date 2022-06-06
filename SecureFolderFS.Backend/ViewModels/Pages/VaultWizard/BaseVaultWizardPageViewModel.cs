using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public abstract class BaseVaultWizardPageViewModel : ObservableObject, IDisposable
    {
        public IMessenger Messenger { get; }

        protected BaseVaultWizardPageViewModel? NextViewModel { get; set; }

        public VaultWizardDialogViewModel DialogViewModel { get; }

        public bool CanGoBack { get; protected init; }

        protected BaseVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
        {
            Messenger = messenger;
            DialogViewModel = dialogViewModel;
            CanGoBack = true;
        }

        public virtual void ReturnToViewModel() { }

        public virtual Task PrimaryButtonClick(IEventDispatchFlag? flag) => Task.CompletedTask;

        public virtual Task SecondaryButtonClick(IEventDispatchFlag? flag) => Task.CompletedTask;

        public virtual void Dispose() { }
    }
}
