using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceHost
{
    public sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>, IRecipient<AddVaultMessage>
    {
        private bool _isInitialized;
        private bool _isCompactMode; // WINDOWS only

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public MainAppHostControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void Receive(RemoveVaultMessage message)
        {
            if (ViewModel?.VaultListViewModel.Items.IsEmpty() ?? false)
                Navigation?.ClearContent();
        }

        /// <inheritdoc/>
        public void Receive(AddVaultMessage message)
        {
#if WINDOWS // TODO(u win)
            if (ViewModel?.VaultListViewModel.Items.Count >= SecureFolderFS.Sdk.Constants.Vault.MAX_FREE_AMOUNT_OF_VAULTS
                && !SettingsService.AppSettings.WasBetaNotificationShown1)
            {
                BetaTeachingTip.IsOpen = true;
            }
#endif
        }

        private async Task NavigateToItem(IVaultModel vaultModel)
        {
            await SetupNavigationAsync();
            if (ViewModel is null)
                return;
            
            // Find existing target or create new
            var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as BaseVaultViewModel)?.VaultModel.Equals(vaultModel) ?? false);
            if (target is null)
            {
                var vaultLoginViewModel = new VaultLoginViewModel(vaultModel);
                vaultLoginViewModel.NavigationRequested += VaultLoginViewModel_NavigationRequested;

                target = new VaultLoginViewModel(vaultModel);
                if (target is IAsyncInitialize asyncInitialize)
                    await asyncInitialize.InitAsync();
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async Task SetupNavigationAsync()
        {
            if (ViewModel is null)
                return;

            ViewModel.NavigationService.SetupNavigation(Navigation);
            if (!_isInitialized)
            {
                _isInitialized = true;
                await ViewModel.InitAsync();
            }
        }

        private async void VaultLoginViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (sender is BaseVaultViewModel viewModel)
                viewModel.NavigationRequested -= VaultLoginViewModel_NavigationRequested;

            if (e is UnlockNavigationRequestedEventArgs args)
            {
                var dashboardViewModel = new VaultDashboardViewModel(args.UnlockedVaultViewModel);
                _ = dashboardViewModel.InitAsync();

                await ViewModel.NavigationService.NavigateAsync(dashboardViewModel);
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
            if (args.SelectedItem is VaultListItemViewModel itemViewModel) 
                await NavigateToItem(itemViewModel.VaultModel);
        }

        private async void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel!.VaultListViewModel.Items.FirstOrDefault(x => x.VaultModel.VaultName.Equals(args.ChosenSuggestion));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem  = chosenItem;
            await NavigateToItem(chosenItem.VaultModel);
        }

        private async void SidebarSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                await ViewModel!.VaultListViewModel.SearchViewModel.SubmitQueryAsync(sender.Text);
        }

        private async void TeachingTip_CloseButtonClick(TeachingTip sender, object args)
        {
            SettingsService.AppSettings.WasBetaNotificationShown1 = true;
            await SettingsService.AppSettings.TrySaveAsync();
        }

        private async void Sidebar_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
#if WINDOWS
            var previous = Sidebar.IsPaneVisible;
            Sidebar.IsPaneVisible = args.DisplayMode != NavigationViewDisplayMode.Minimal;
            _isCompactMode = !Sidebar.IsPaneVisible;

            if (Sidebar.IsPaneVisible && Sidebar.IsPaneVisible != previous)
            {
                Sidebar.IsPaneOpen = true;
                Sidebar.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            }

            if (_isCompactMode)
            {
                Sidebar.IsPaneVisible = false;
                PaneShowButton.Visibility = Visibility.Visible;
                await Task.Delay(1000);
                PaneShowButton.Visibility = Visibility.Collapsed;
            }
            else
                PaneShowButton.Visibility = Visibility.Collapsed;
#endif
        }

        private async void Sidebar_PaneClosed(NavigationView sender, object args)
        {
#if WINDOWS
            if (_isCompactMode)
            {
                Sidebar.IsPaneVisible = false;
                PaneShowButton.Visibility = Visibility.Visible;
                await Task.Delay(1000);
                PaneShowButton.Visibility = Visibility.Collapsed;
            }
#endif
        }

        private void PaneShowButton_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            Sidebar.IsPaneVisible = true;
            Sidebar.IsPaneOpen = true;
            PaneShowButton.Visibility = Visibility.Collapsed;
#endif
        }

        private void PaneButtonGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
#if WINDOWS
            if (_isCompactMode)
                PaneShowButton.Visibility = Visibility.Visible;
#endif
        }

        private void PaneButtonGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
#if WINDOWS
            if (_isCompactMode)
                PaneShowButton.Visibility = Visibility.Collapsed;
#endif
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
