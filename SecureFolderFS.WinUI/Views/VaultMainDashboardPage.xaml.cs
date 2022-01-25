using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultMainDashboardPage : Page
    {
        public VaultMainDashboardPageViewModel ViewModel
        {
            get => (VaultMainDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultMainDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as DashboardPageNavigationParameterModel)?.ViewModel is VaultMainDashboardPageViewModel vaultMainDashboardPageViewModel)
            {
                this.ViewModel = vaultMainDashboardPageViewModel;
            }

            base.OnNavigatedTo(e);
        }
    }
}
