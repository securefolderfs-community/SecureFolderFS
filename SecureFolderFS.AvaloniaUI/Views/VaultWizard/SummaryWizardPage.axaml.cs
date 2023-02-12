using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class SummaryWizardPage : Page
    {
        public VaultWizardSummaryViewModel? ViewModel
        {
            get => (VaultWizardSummaryViewModel?)DataContext;
            set => DataContext = value;
        }

        public SummaryWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
            // TODO OnThemeChangedEvent
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardSummaryViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}