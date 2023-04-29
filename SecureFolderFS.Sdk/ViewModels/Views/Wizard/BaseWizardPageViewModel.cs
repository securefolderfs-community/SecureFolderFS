using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    public abstract class BaseWizardPageViewModel : BasePageViewModel
    {
        protected VaultWizardDialogViewModel DialogViewModel { get; }

        protected INavigationService NavigationService => DialogViewModel.NavigationService;

        protected BaseWizardPageViewModel(VaultWizardDialogViewModel dialogViewModel)
        {
            DialogViewModel = dialogViewModel;
        }

        public virtual Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual Task SecondaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
