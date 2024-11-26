using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.DebugControls
{
    [INotifyPropertyChanged]
    public sealed partial class DebugDashboardRepresentationControl : UserControl
    {
        private MainWindowRootControl? _rootControl;
        private INavigationControl? _rootNavigationControl;
        private VaultOverviewViewModel? _overviewViewModel;
        private VaultPropertiesViewModel? _propertiesViewModel;
        private GraphsWidgetViewModel? _graphsWidgetViewModel;
        private HealthWidgetViewModel? _healthWidgetViewModel;

        [ObservableProperty] private bool _IsOverviewVisible;
        [ObservableProperty] private bool _IsPropertiesVisible;

        public DebugDashboardRepresentationControl()
        {
            InitializeComponent();

            _rootControl = App.Instance?.MainWindow?.Content as MainWindowRootControl;
            _rootNavigationControl = (_rootControl?.RootNavigationService as INavigationControlContract)?.Navigator;
            ResetView();
        }

        private void ResetView()
        {
            if (_rootControl is null)
                return;

            if (_rootControl.RootNavigationService.CurrentView is not MainHostViewModel mainHost)
                return;

            if (mainHost.NavigationService.CurrentView is not VaultDashboardViewModel vaultDashboard)
                return;

            _overviewViewModel = null;
            _propertiesViewModel = null;
            IsOverviewVisible = false;
            IsPropertiesVisible = false;

            if (vaultDashboard.DashboardNavigation.CurrentView is VaultOverviewViewModel vaultOverview)
                _ = PresentVaultOverview(vaultOverview);

            if (vaultDashboard.DashboardNavigation.CurrentView is VaultPropertiesViewModel vaultProperties)
                _ = PresentVaultProperties(vaultProperties);
        }

        private async Task PresentVaultOverview(VaultOverviewViewModel viewModel)
        {
            _overviewViewModel = viewModel;
            IsOverviewVisible = true;

            _graphsWidgetViewModel = _overviewViewModel?.WidgetsViewModel.Widgets.FirstOrDefault(x => x is GraphsWidgetViewModel) as GraphsWidgetViewModel;
            _healthWidgetViewModel = _overviewViewModel?.WidgetsViewModel.Widgets.FirstOrDefault(x => x is HealthWidgetViewModel) as HealthWidgetViewModel;

            await Task.Delay(1000);
            Dbg_HealthWidget_DateChecked.Text = _healthWidgetViewModel?.LastCheckedText;
        }

        private async Task PresentVaultProperties(VaultPropertiesViewModel viewModel)
        {
            _propertiesViewModel = viewModel;
            IsPropertiesVisible = true;

            await Task.CompletedTask;
        }

        private void SetGraphsViewMockup_Click(object sender, RoutedEventArgs e)
        {
            if (_graphsWidgetViewModel is null)
                return;

            _graphsWidgetViewModel.IsActive = false;
            _graphsWidgetViewModel.ReadGraphViewModel.GraphSubHeader = "209mb/s";
            _graphsWidgetViewModel.ReadGraphViewModel.Data.Clear();
            _graphsWidgetViewModel.ReadGraphViewModel.Data.AddMultiple([ 4, 3, 6, 3, 20, 12, 30, 39 ]);

            _graphsWidgetViewModel.WriteGraphViewModel.GraphSubHeader = "104,8mb/s";
            _graphsWidgetViewModel.WriteGraphViewModel.Data.Clear();
            _graphsWidgetViewModel.WriteGraphViewModel.Data.AddMultiple([ 4, 3, 6, 3, 20, 12, 30, 39]);
        }

        private void RestoreGraphs_Click(object sender, RoutedEventArgs e)
        {
            if (_graphsWidgetViewModel is null)
                return;

            _graphsWidgetViewModel.IsActive = true;
        }

        private void HealthWidgetDateChecked_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_healthWidgetViewModel is null)
                return;

            _healthWidgetViewModel.LastCheckedText = Dbg_HealthWidget_DateChecked.Text;
        }
    }
}
