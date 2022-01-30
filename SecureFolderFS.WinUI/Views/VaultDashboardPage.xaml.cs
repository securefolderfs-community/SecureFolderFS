using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultDashboardPage : Page, IRecipient<DashboardNavigationFinishedMessage>
    {
        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Register<DashboardNavigationFinishedMessage>(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as PageNavigationParameterModel)?.ViewModel is VaultDashboardPageViewModel vaultDashboardPageViewModel)
            {
                ViewModel = vaultDashboardPageViewModel;

                NavigatePage(ViewModel.BaseDashboardPageViewModel!, null);
            }

            base.OnNavigatedTo(e);
        }

        public void Receive(DashboardNavigationFinishedMessage message)
        {
            NavigatePage(message.Value, message.From);
        }

        private void NavigatePage(BaseDashboardPageViewModel baseDashboardPageViewModel, string? senderFrom)
        {
            switch (baseDashboardPageViewModel)
            {
                case VaultMainDashboardPageViewModel:
                    NavigationTransitionInfo transition = !string.IsNullOrEmpty(senderFrom) && senderFrom != "Properties" ? new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft} : new ContinuumNavigationTransitionInfo();
                    ContentFrame.Navigate(typeof(VaultMainDashboardPage), new DashboardPageNavigationParameterModel() { ViewModel = baseDashboardPageViewModel }, transition);
                    break;

                case VaultDashboardPropertiesPageViewModel:
                    ContentFrame.Navigate(typeof(VaultDashboardPropertiesPage), new DashboardPageNavigationParameterModel() { ViewModel = baseDashboardPageViewModel }, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
                    break;
            }
        }

        private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is DashboardNavigationItemViewModel dashboardNavigationItemViewModel)
            {
                dashboardNavigationItemViewModel.NavigationAction?.Invoke(ViewModel.DashboardNavigationViewModel.DashboardNavigationItems.FirstOrDefault());
            }
        }
    }
}
