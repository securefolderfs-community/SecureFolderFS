using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class CreationPathWizardPage : Page
    {
        public VaultWizardCreationPathViewModel? ViewModel
        {
            get => (VaultWizardCreationPathViewModel?)DataContext;
            set => DataContext = value;
        }

        public CreationPathWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardCreationPathViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}