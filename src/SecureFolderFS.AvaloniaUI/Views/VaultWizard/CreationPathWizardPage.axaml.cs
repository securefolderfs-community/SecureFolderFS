using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class CreationPathWizardPage : Page
    {
        public NewLocationWizardViewModel? ViewModel
        {
            get => (NewLocationWizardViewModel?)DataContext;
            set => DataContext = value;
        }

        public CreationPathWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NewLocationWizardViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}