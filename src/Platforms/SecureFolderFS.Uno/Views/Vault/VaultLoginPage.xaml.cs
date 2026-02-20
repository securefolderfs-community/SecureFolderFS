using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.UserControls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultLoginPage : Page
    {
        public VaultLoginViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultLoginViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultLoginPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultLoginViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;
                ViewModel.NavigationRequested += ViewModel_NavigationRequested;
            }

            base.OnNavigatedTo(e);
        }
        
        private async void LoginControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(300); // Wait for the UI to update
            var contentRoot = LoginControl.LoginContentControl.GetContentControlRoot();
            if ((contentRoot?.FindChild<PasswordControl>()?.Focus(FocusState.Programmatic) ?? false)
                || (contentRoot?.FindChild<Button>()?.Focus(FocusState.Programmatic) ?? false))
                _ = 0;
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e is not UnlockNavigationRequestedEventArgs args)
                return;

            var dashboardNavigation = DI.Service<INavigationService>();
            var dashboardViewModel = new VaultDashboardViewModel(args.UnlockedVaultViewModel, ViewModel.VaultNavigation, dashboardNavigation);
            ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            await ViewModel.VaultNavigation.ForgetNavigateSpecificViewAsync(dashboardViewModel,
               x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(args.UnlockedVaultViewModel.VaultViewModel.VaultModel) ?? false);
        }
    }
}
