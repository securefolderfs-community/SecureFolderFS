using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Shared.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultDashboardPage : Page //, IRecipient<VaultLockedMessage>
    {
        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            InitializeComponent();
        }

        //public async void Receive(VaultLockedMessage message)
        //{
        //    // Await and change the visibility so the page doesn't prevail on the lock animation
        //    await Task.Delay(100);
        //    Visibility = Visibility.Collapsed;
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardPageViewModel viewModel)
                ViewModel = viewModel;

            Navigation.Navigate(ViewModel.CurrentPage, new EntranceNavigationTransitionInfo());

            base.OnNavigatedTo(e);
        }

        //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    (ViewModel as ICleanable)?.Cleanup();

        //    ViewModel.Messenger.Unregister<DashboardNavigationFinishedMessage>(this);
        //    ViewModel.Messenger.Unregister<VaultLockedMessage>(this);

        //    base.OnNavigatingFrom(e);
        //}

        //private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        //{
        //    if (args.Item is NavigationItemViewModel itemViewModel)
        //    {
        //        itemViewModel.NavigationAction?.Invoke(ViewModel.NavigationBreadcrumbViewModel.DashboardNavigationItems.FirstOrDefault());
        //    }
        //}
    }
}
