using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Backend.ViewModels.Sidebar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindowHostPage : Page
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
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
            {
                WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(itemViewModel.VaultViewModel) { Transition = new EntranceTransitionModel() });
            }
        }

        private async void MainWindowHostPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.EnsureLateApplication();
        }
    }
}
