using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class SummaryWizardPage : Page
    {
        public SummaryWizardViewModel? ViewModel
        {
            get => (SummaryWizardViewModel?)DataContext;
            set => DataContext = value;
        }

        public SummaryWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
            // TODO OnThemeChangedEvent
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SummaryWizardViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}