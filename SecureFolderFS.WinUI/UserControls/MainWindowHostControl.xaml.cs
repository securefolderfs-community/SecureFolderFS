using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class MainWindowHostControl : UserControl, IRecipient<RemoveVaultMessage>
    {
        public MainViewModel ViewModel
        {
            get => (MainViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindowHostControl()
        {
            InitializeComponent();
            ViewModel = new();
            WeakReferenceMessenger.Default.Register<RemoveVaultMessage>(this);
        }

        private void NavigateToItem(IVaultModel vaultModel)
        {
            // Get the item from cache or create new instance
            if (!Navigation.NavigationCache.TryGetValue(vaultModel, out var destination))
                destination = new VaultLoginPageViewModel(vaultModel);

            // Navigate
            Navigation.Navigate(destination, new EntranceNavigationTransitionInfo());

            _ = destination.InitAsync();
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
                NavigateToItem(itemViewModel.VaultModel);
        }

        private void MainWindowHostControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(Navigation);

            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var threadingService = Ioc.Default.GetRequiredService<IThreadingService>();

            _ = settingsService.LoadSettingsAsync().ContinueWith(async _ =>
            {
                await threadingService.ExecuteOnUiThreadAsync();
                await ViewModel.InitAsync();

                Sidebar.SelectedItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault();
            });
        }

        private void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultModel.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem  = chosenItem;
            NavigateToItem(chosenItem.VaultModel);
        }

        private async void SidebarSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery(sender.Text);
        }

        public void Receive(RemoveVaultMessage message)
        {
            if (ViewModel.SidebarViewModel.SidebarItems.IsEmpty())
                Navigation.ClearContent();

            Navigation.NavigationCache.Remove(message.VaultModel);
        }
    }
}
