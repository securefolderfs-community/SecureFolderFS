using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.AppHost;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using SecureFolderFS.Shared.Extensions;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.InterfaceHost
{
    public sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>
    {
        public MainAppHostControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Receive(RemoveVaultMessage message)
        {
            if (ViewModel.SidebarViewModel.SidebarItems.IsEmpty())
                Navigation.ClearContent();

            Navigation.NavigationCache.Remove(message.VaultModel);
        }

        private void NavigateToItem(IVaultModel vaultModel)
        {
            // Get the item from cache or create new instance
            if (!Navigation.NavigationCache.TryGetValue(vaultModel, out var destination))
            {
                destination = new VaultLoginPageViewModel(vaultModel);
                _ = destination.InitAsync();
            }

            // Navigate
            Navigation.Navigate(destination, new EntranceNavigationTransitionInfo());
        }

        private async void MainAppHostControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register(this);
            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(Navigation);

            await ViewModel.InitAsync();
            Sidebar.SelectedItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault();
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
                NavigateToItem(itemViewModel.VaultModel);
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

        public MainAppHostViewModel ViewModel
        {
            get => (MainAppHostViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainAppHostViewModel), typeof(MainAppHostControl), new PropertyMetadata(null));
    }
}
