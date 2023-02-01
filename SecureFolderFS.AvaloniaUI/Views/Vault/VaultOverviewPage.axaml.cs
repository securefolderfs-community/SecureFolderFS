using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultOverviewPage : Page
    {
        public VaultOverviewPageViewModel ViewModel
        {
            get => (VaultOverviewPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultOverviewPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultOverviewPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}