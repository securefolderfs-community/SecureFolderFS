using System;
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
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    public sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>, IRecipient<AddVaultMessage>
    {
        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();
        
        public MainAppHostControl()
        {
            AvaloniaXamlLoader.Load(this);

            // SettingsButton.AddHandler(PointerPressedEvent, SettingsButton_PointerPressed, handledEventsToo: true);
            // SettingsButton.AddHandler(PointerReleasedEvent, SettingsButton_PointerReleased, handledEventsToo: true);
        }

        /// <inheritdoc/>
        public void Receive(RemoveVaultMessage message)
        {
            if (ViewModel.SidebarViewModel.SidebarItems.IsEmpty())
                Navigation.ClearContent();
        }
        
        /// <inheritdoc/>
        public void Receive(AddVaultMessage message)
        {
            if (ViewModel.SidebarViewModel.SidebarItems.Count >= Sdk.Constants.Vault.MAX_FREE_AMOUNT_OF_VAULTS
                && !SettingsService.AppSettings.WasBetaNotificationShown1)
            {
                BetaTeachingTip.IsOpen = true;
            }
        }

        private async Task NavigateToItem(VaultViewModel vaultViewModel)
        {
            // Find existing target or create new
            var target = ViewModel.NavigationService.Targets.FirstOrDefault(x => (x as BaseVaultPageViewModel)?.VaultViewModel == vaultViewModel);
            target ??= new VaultLoginPageViewModel(vaultViewModel, ViewModel.NavigationService);

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async void MainAppHostControl_Loaded(object? sender, RoutedEventArgs e)
        {
            ViewModel.NavigationService.SetupNavigation(Navigation);
            WeakReferenceMessenger.Default.Register<RemoveVaultMessage>(this);
            WeakReferenceMessenger.Default.Register<AddVaultMessage>(this);

            await ViewModel.InitAsync();
            Sidebar.SelectedItem = ViewModel.SidebarViewModel.SelectedItem;
        }

        private async void Sidebar_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is SidebarItemViewModel itemViewModel)
                await NavigateToItem(itemViewModel.VaultViewModel);
        }

        private async void SidebarSearchBox_TextChanged(object? sender, TextChangedEventArgs args)
        {
            await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery((sender as AutoCompleteBox)?.Text ?? string.Empty);
        }

        private async void SidebarSearchBox_SelectionChanged(object? sender, SelectionChangedEventArgs args)
        {
            // TODO Fix crash
            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.VaultName.Equals(((AutoCompleteBox)sender).SelectedItem?.ToString()));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem = chosenItem;
            await NavigateToItem(chosenItem.VaultViewModel);
        }

        // private void SettingsButton_PointerPressed(object? sender, PointerPressedEventArgs e)
        // {
        //     SpinSettingsIconPointerPressedStoryboard.BeginAsync();
        // }
        //
        // private void SettingsButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
        // {
        //     SpinSettingsIconPointerReleasedStoryboard.BeginAsync();
        // }
        
        private async void TeachingTip_CloseButtonClick(TeachingTip sender, EventArgs args)
        {
            SettingsService.AppSettings.WasBetaNotificationShown1 = true;
            await SettingsService.AppSettings.SaveAsync();
        }

        public MainHostViewModel ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly StyledProperty<MainHostViewModel> ViewModelProperty =
            AvaloniaProperty.Register<MainAppHostControl, MainHostViewModel>(nameof(ViewModel));
    }
}