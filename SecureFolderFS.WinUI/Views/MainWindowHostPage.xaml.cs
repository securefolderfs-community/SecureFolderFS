using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.Backend.ViewModels.Sidebar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindowHostPage : Page, IRecipient<NavigationRequestedMessage>
    {
        public MainViewModel ViewModel
        {
            get => (MainViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindowHostPage()
        {
            this.InitializeComponent();

            this.ViewModel = new();

            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this);
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel sidebarItemViewModel)
            {
                ViewModel.NavigationModel.NavigateToPage(sidebarItemViewModel.VaultModel);
            }
        }

        public void Receive(NavigationRequestedMessage message)
        {
            switch (message.Value)
            {
                case VaultLoginPageViewModel:
                    ContentFrame.Navigate(typeof(VaultLoginPage), new PageNavigationParameterModel() { ViewModel = message.Value }, new EntranceNavigationTransitionInfo());
                    break;

                case VaultDashboardPageViewModel:
                    ContentFrame.Navigate(typeof(VaultDashboardPage), new PageNavigationParameterModel() { ViewModel = message.Value }, new SlideNavigationTransitionInfo());
                    break;
            }
        }
    }
}
