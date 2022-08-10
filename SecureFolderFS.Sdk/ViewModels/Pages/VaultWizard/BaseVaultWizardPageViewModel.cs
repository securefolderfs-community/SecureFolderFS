using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public abstract class BaseVaultWizardPageViewModel : ObservableObject
    {
        protected IMessenger Messenger { get; }

        protected VaultWizardDialogViewModel DialogViewModel { get; }

        protected BaseVaultWizardPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
        {
            Messenger = messenger;
            DialogViewModel = dialogViewModel;
        }

        public virtual Task PrimaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual Task SecondaryButtonClickAsync(IEventDispatchFlag? flag, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
