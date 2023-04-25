using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.WinUI.ServiceImplementation;
using System.Linq;
using System.Threading.Tasks;

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
        }

        private async Task NavigateToItem(VaultViewModel vaultViewModel)
        {
            // Find existing target or create new
            var target = ViewModel.NavigationService.Targets.FirstOrDefault(x => (x as BaseVaultPageViewModel)?.VaultViewModel == vaultViewModel);
            target ??= new VaultLoginPageViewModel(vaultViewModel, ViewModel.NavigationService);

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private bool SetupNavigation()
        {
            if (ViewModel.NavigationService.IsInitialized)
                return true;

            if (ViewModel.NavigationService is WindowsNavigationService navigationServiceImpl)
            {
                navigationServiceImpl.NavigationControl = Navigation;
                return navigationServiceImpl.NavigationControl is not null;
            }

            return false;
        }

        private async void MainAppHostControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetupNavigation();
            WeakReferenceMessenger.Default.Register(this);

            await ViewModel.InitAsync();
            Sidebar.SelectedItem = ViewModel.SidebarViewModel.SelectedItem;
        }

        private async void Sidebar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is SidebarItemViewModel itemViewModel) 
                await NavigateToItem(itemViewModel.VaultViewModel);
        }

        private async void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem  = chosenItem;
            await NavigateToItem(chosenItem.VaultViewModel);
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
