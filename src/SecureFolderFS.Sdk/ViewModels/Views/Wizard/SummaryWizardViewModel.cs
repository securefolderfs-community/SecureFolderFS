using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    public sealed partial class SummaryWizardViewModel : BaseWizardPageViewModel
    {
        [ObservableProperty] private string _VaultName;

        public SummaryWizardViewModel(string vaultName, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = true;
            _VaultName = vaultName;
        }
    }
}
