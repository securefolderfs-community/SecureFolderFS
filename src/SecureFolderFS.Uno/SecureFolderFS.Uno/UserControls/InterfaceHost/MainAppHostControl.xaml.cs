using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.UI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceHost
{
    public sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>, IRecipient<AddVaultMessage>
    {
        private bool _isInitialized;

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public MainAppHostControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Receive(RemoveVaultMessage message)
        {
            if (ViewModel?.SidebarViewModel.SidebarItems.IsEmpty() ?? false)
                Navigation?.ClearContent();
        }

        /// <inheritdoc/>
        public void Receive(AddVaultMessage message)
        {
#if WINDOWS // TODO(u win)
            if (ViewModel?.SidebarViewModel.SidebarItems.Count >= SecureFolderFS.Sdk.Constants.Vault.MAX_FREE_AMOUNT_OF_VAULTS
                && !SettingsService.AppSettings.WasBetaNotificationShown1)
            {
                BetaTeachingTip.IsOpen = true;
            }
#endif
        }

        private async Task NavigateToItem(VaultViewModel vaultViewModel)
        {
            await SetupNavigationAsync();
            if (ViewModel is null)
                return;
            
            // Find existing target or create new
            var target = ViewModel.NavigationService.Targets.FirstOrDefault(x => (x as BaseVaultPageViewModel)?.VaultViewModel == vaultViewModel);
            if (target is null)
            {
                target = new VaultLoginPageViewModel(vaultViewModel, ViewModel.NavigationService);
                if (target is IAsyncInitialize asyncInitialize)
                    await asyncInitialize.InitAsync();
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async Task SetupNavigationAsync()
        {
            ViewModel?.NavigationService.SetupNavigation(Navigation);
            if (!_isInitialized && ViewModel is not null)
            {
                _isInitialized = true;
                await ViewModel.InitAsync();
            }
        }

        private async void MainAppHostControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<RemoveVaultMessage>(this);
            WeakReferenceMessenger.Default.Register<AddVaultMessage>(this);

            await SetupNavigationAsync();
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

        private async void TeachingTip_CloseButtonClick(TeachingTip sender, object args)
        {
            SettingsService.AppSettings.WasBetaNotificationShown1 = true;
            await SettingsService.AppSettings.TrySaveAsync();
        }

        public MainHostViewModel? ViewModel
        {
            get => (MainHostViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainHostViewModel), typeof(MainAppHostControl), new PropertyMetadata(null,
                (s, e) =>
                {
                    if (s is MainAppHostControl sender)
                        _ = sender.SetupNavigationAsync();
                }));
    }
}
