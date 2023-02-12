using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.ExistingVault;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class AddExistingWizardPage : Page
    {
        public VaultWizardSelectLocationViewModel? ViewModel
        {
            get => (VaultWizardSelectLocationViewModel?)DataContext;
            set => DataContext = value;
        }

        public AddExistingWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardSelectLocationViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}