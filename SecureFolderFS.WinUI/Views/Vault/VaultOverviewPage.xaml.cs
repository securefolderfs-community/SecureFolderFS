using System.Threading;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultOverviewPage : Page
    {
        private readonly SemaphoreSlim _graphClickSemaphore;

        public VaultOverviewPageViewModel ViewModel
        {
            get => (VaultOverviewPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultOverviewPage()
        {
            InitializeComponent();

            _graphClickSemaphore = new(1, 1);
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultOverviewPageViewModel viewModel)
            {
                ViewModel = viewModel;
                Graphs.RestoreGraphsState();
            }

            base.OnNavigatedTo(e);
        }

        /// <inheritdoc/>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _graphClickSemaphore.Dispose();
            Graphs?.Dispose();

            base.OnNavigatingFrom(e);
        }
    }
}
