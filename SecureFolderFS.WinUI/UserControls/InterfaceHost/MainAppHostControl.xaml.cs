using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
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

            var viewModelToRemove = Navigation.NavigationCache.Keys.FirstOrDefault(x => x.VaultModel.Equals(message.VaultModel));
            if (viewModelToRemove is null)
                return;

            Navigation.NavigationCache.Remove(viewModelToRemove);
        }

        private void NavigateToItem(VaultViewModel vaultViewModel)
        {
            // Get the item from cache or create new instance
            if (!Navigation.NavigationCache.TryGetValue(vaultViewModel, out var destination))
            {
                destination = new VaultLoginPageViewModel(vaultViewModel);
                _ = destination.InitAsync();
            }

            // Navigate
            Navigation.Navigate(destination, new EntranceNavigationTransitionInfo());
        }

        private async void MainAppHostControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register(this);
            WeakReferenceMessenger.Default.Register<NavigationMessage>(Navigation);

            await ViewModel.InitAsync();
            Sidebar.SelectedItem = ViewModel.SidebarViewModel.SelectedItem;
        }

        private void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel)
                NavigateToItem(itemViewModel.VaultViewModel);
        }

        private void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem  = chosenItem;
            NavigateToItem(chosenItem.VaultViewModel);
        }

        private async void SidebarSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery(sender.Text);
        }

        public MainHostViewModel ViewModel
        {
            get => (MainHostViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainHostViewModel), typeof(MainAppHostControl), new PropertyMetadata(null));
    }
}
