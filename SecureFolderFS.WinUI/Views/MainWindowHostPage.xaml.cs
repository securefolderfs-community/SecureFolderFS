using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
            InitializeComponent();

            ViewModel = new();
        }

        private void NavigateToItem(IVaultModel vaultModel)
        {
            Navigation.NavigationCache.FirstOrDefault(x => vaultModel.Equals(x));// TODO
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
            {
                NavigateToItem(itemViewModel.VaultModel);
            }
        }

        private void MainWindowHostPage_Loaded(object sender, RoutedEventArgs e)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var threadingService = Ioc.Default.GetRequiredService<IThreadingService>();

            _ = settingsService.LoadSettingsAsync().ContinueWith(async _ =>
            {
                await threadingService.ExecuteOnUiThreadAsync();
                await ViewModel.InitAsync();

                ViewModel.SidebarViewModel.SelectedItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault();
                if (ViewModel.SidebarViewModel.SelectedItem is not null)
                    WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(ViewModel.SidebarViewModel.SelectedItem.VaultModel));
            });
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
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultModel.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is not null)
            {
                ViewModel.SidebarViewModel.SelectedItem = chosenItem;
                NavigateToItem(chosenItem.VaultModel);
            }
        }
    }
}
