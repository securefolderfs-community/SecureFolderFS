using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class MainWizardPage : Page
    {
        public MainVaultWizardPageViewModel ViewModel
        {
            get => (MainVaultWizardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWizardPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainVaultWizardPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}