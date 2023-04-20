using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    public sealed partial class MainWizardPageViewModel : BaseWizardPageViewModel
    {
        public MainWizardPageViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
        }

        [RelayCommand]
        private async Task AddExistingVaultAsync()
        {
            await NavigationService.TryNavigateAsync(() => new ExistingLocationWizardViewModel(DialogViewModel));
        }

        [RelayCommand]
        private async Task CreateNewVaultAsync()
        {
            await NavigationService.TryNavigateAsync(() => new NewLocationWizardViewModel(DialogViewModel));
        }
    }
}
