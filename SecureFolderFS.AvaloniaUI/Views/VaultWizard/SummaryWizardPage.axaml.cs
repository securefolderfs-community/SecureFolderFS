using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class SummaryWizardPage : Page
    {
        public VaultWizardSummaryViewModel ViewModel
        {
            get => (VaultWizardSummaryViewModel)DataContext;
            set => DataContext = value;
        }

        public SummaryWizardPage()
        {
            InitializeComponent();
            // TODO OnThemeChangedEvent
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardSummaryViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }
    }
}