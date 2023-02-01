using System.Collections.ObjectModel;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultDashboardPage : Page
    {
        public ObservableCollection<OrderedBreadcrumbBarItem> BreadcrumbItems { get; }

        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            InitializeComponent();
            BreadcrumbItems = new();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardPageViewModel viewModel)
                ViewModel = viewModel;

            Navigation.Navigate(ViewModel.CurrentPage, new EntranceNavigationTransition());
            BreadcrumbItems.Add(new(ViewModel.VaultViewModel.VaultModel.VaultName, true));

            base.OnNavigatedTo(e);
        }
    }
}