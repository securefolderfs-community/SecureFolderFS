using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardFinishPageViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardFinishPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this.VaultName = DialogViewModel.VaultViewModel!.VaultName;

            DialogViewModel.IsPrimaryButtonEnabled = true;
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<HandledCallback?>(PrimaryButtonClick);
        }

        private void PrimaryButtonClick(HandledCallback? e)
        {

        }
    }
}
