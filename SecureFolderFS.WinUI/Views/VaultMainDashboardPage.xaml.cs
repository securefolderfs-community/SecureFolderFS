using System.Threading;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultMainDashboardPage : Page
    {
        private readonly SemaphoreSlim _graphClickSemaphore;

        public VaultMainDashboardPageViewModel ViewModel
        {
            get => (VaultMainDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultMainDashboardPage()
        {
            InitializeComponent();

            _graphClickSemaphore = new(1, 1);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultMainDashboardPageViewModel viewModel)
            {
                ViewModel = viewModel;

                Graphs.RestoreGraphsState();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _graphClickSemaphore.Dispose();
            Graphs?.Dispose();

            ViewModel.Cleanup();

            base.OnNavigatingFrom(e);
        }
    }
}
