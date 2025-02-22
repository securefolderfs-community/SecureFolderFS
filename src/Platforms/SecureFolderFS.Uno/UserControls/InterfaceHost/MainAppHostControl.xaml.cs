using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.UI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceHost
{
    public sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>, IRecipient<AddVaultMessage>
    {
        private bool _isInitialized;
        private bool _isCompactMode; // WINDOWS only

        private ISettingsService SettingsService { get; } = DI.Service<ISettingsService>();

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
#if WINDOWS
            if (ViewModel?.VaultListViewModel.Items.Count >= SecureFolderFS.Sdk.Constants.Vault.MAX_FREE_AMOUNT_OF_VAULTS
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
            var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(vaultViewModel.VaultModel) ?? false);
            if (target is null)
            {
                var vaultLoginViewModel = new VaultLoginViewModel(vaultViewModel, ViewModel.NavigationService);
                _ = vaultLoginViewModel.InitAsync();
                target = vaultLoginViewModel;
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

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem menuItem)
                return;

            if (menuItem is not { DataContext: VaultListItemViewModel itemViewModel })
                return;

            itemViewModel.IsRenaming = true;
        }

        private async void RenameBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not TextBox { DataContext: VaultListItemViewModel itemViewModel } textBox)
                return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                {
                    e.Handled = true;
                    await itemViewModel.RenameCommand.ExecuteAsync(textBox.Text);
                    break;
                }

                case VirtualKey.Escape:
                {
                    itemViewModel.IsRenaming = false;
                    break;
                }
            }
        }

        private void RenameBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox { DataContext: VaultListItemViewModel itemViewModel } textBox)
                return;

            textBox.Focus(FocusState.Programmatic);
            textBox.Text = itemViewModel.VaultViewModel.Title;
            textBox.SelectAll();
        }

        private void RenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox { DataContext: VaultListItemViewModel itemViewModel })
                return;

            itemViewModel.IsRenaming = false;
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
                await NavigateToItem(itemViewModel.VaultViewModel);
        }

        private async void SidebarSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenItem = ViewModel!.VaultListViewModel.Items.FirstOrDefault(x => x.VaultViewModel.Title.Equals(args.ChosenSuggestion));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem  = chosenItem;
            await NavigateToItem(chosenItem.VaultViewModel);
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

        #region Drag and Drop

        private void Sidebar_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
                e.AcceptedOperation = DataPackageOperation.Link;
        }

        private async void Sidebar_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                return;

            // We only want to get the first item
            // as it is unlikely the user will want to add multiple vaults in batch.
            var droppedItems = await e.DataView.GetStorageItemsAsync().AsTask();
            var item = droppedItems.FirstOrDefault();
            if (item is null)
                return;

            // Recreate as SystemFolder for best performance.
            // The logic can be changed to handle Platform Storage Items in the future, if needed.
            var folder = new SystemFolderEx(item.Path);
            await ViewModel.VaultListViewModel.AddNewVaultCommand.ExecuteAsync(folder);
        }

        #endregion

        #region Sidebar Handle Actions

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
#else
            await Task.CompletedTask;
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
#else
            await Task.CompletedTask;
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

        #endregion

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
        
        private void NavigationItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
#if HAS_UNO
            if (sender is not NavigationViewItem navigationViewItem)
                return;
            
            navigationViewItem.ContextFlyout.ShowAt(navigationViewItem);
#endif
        }
    }
}
