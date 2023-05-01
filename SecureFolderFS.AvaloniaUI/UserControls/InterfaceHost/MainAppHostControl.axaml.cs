using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    internal sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>
    {
        public MainAppHostControl()
        {
            AvaloniaXamlLoader.Load(this);

            SettingsButton.AddHandler(PointerPressedEvent, SettingsButton_OnPointerPressed, handledEventsToo: true);
            SettingsButton.AddHandler(PointerReleasedEvent, SettingsButton_OnPointerReleased, handledEventsToo: true);
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

        private async void MainAppHostControl_OnLoaded(object? sender, RoutedEventArgs e)
        {
            ViewModel.NavigationService.SetupNavigation(Navigation);
            WeakReferenceMessenger.Default.Register(this);

            await ViewModel.InitAsync();
            Sidebar.SelectedItem = ViewModel.SidebarViewModel.SelectedItem;
        }

        public MainHostViewModel ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly StyledProperty<MainHostViewModel> ViewModelProperty =
            AvaloniaProperty.Register<MainAppHostControl, MainHostViewModel>(nameof(ViewModel));

        private async void Sidebar_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is SidebarItemViewModel itemViewModel)
                await NavigateToItem(itemViewModel.VaultViewModel);
        }

        private async void AutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery((sender as AutoCompleteBox)?.Text ?? string.Empty);
        }

        private async void AutoCompleteBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
                return;

            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.VaultName.Equals(((AutoCompleteBox)sender).SelectedItem?.ToString()));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem = chosenItem;
            await NavigateToItem(chosenItem.VaultViewModel);
        }

        private void SettingsButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SpinSettingsIconPointerPressedStoryboard.RunAnimationsAsync();
        }

        private void SettingsButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            SpinSettingsIconPointerReleasedStoryboard.RunAnimationsAsync();
        }
    }
}