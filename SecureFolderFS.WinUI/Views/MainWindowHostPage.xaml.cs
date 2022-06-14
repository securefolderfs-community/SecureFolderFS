using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindowHostPage : Page, IRecipient<RemoveVaultRequestedMessage>
    {
        public MainViewModel ViewModel
        {
            get => (MainViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindowHostPage()
        {
            InitializeComponent();

            ViewModel = new();

            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            ViewModel.SidebarViewModel.SelectedItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault();
        }

        private void NavigateToItem(VaultViewModel vaultViewModel)
        {
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(vaultViewModel) { Transition = new EntranceTransitionModel() });
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
            {
                NavigateToItem(itemViewModel.VaultViewModel);
            }
        }

        private void MainWindowHostPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SavedVaultsModel.Initialize();
        }

        private async void SidebarSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery(sender.Text);
            }
        }

        private void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is not null)
            {
                ViewModel.SidebarViewModel.SelectedItem = chosenItem;
                NavigateToItem(chosenItem.VaultViewModel);
            }
        }
    }
}
