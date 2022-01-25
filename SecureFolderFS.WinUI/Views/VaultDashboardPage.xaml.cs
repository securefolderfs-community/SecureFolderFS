using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
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
    public sealed partial class VaultDashboardPage : Page, IRecipient<DashboardNavigationRequestedMessage>
    {
        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as PageNavigationParameterModel)?.ViewModel is VaultDashboardPageViewModel vaultDashboardPageViewModel)
            {
                ViewModel = vaultDashboardPageViewModel;

                NavigatePage(ViewModel.BaseDashboardPageViewModel);
            }

            base.OnNavigatedTo(e);
        }

        public void Receive(DashboardNavigationRequestedMessage message)
        {
            NavigatePage(message.Value);
        }

        private void NavigatePage(BaseDashboardPageViewModel baseDashboardPageViewModel)
        {
            switch (baseDashboardPageViewModel)
            {
                case VaultMainDashboardPageViewModel:
                    ContentFrame.Navigate(typeof(VaultMainDashboardPage), new DashboardPageNavigationParameterModel() { ViewModel = baseDashboardPageViewModel }, new SlideNavigationTransitionInfo());
                    break;
            }
        }
    }
}
