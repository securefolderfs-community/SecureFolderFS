using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using System.Linq;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    internal sealed partial class MainAppHostControl : UserControl, IRecipient<RemoveVaultMessage>
    {
        public MainAppHostControl()
        {
            AvaloniaXamlLoader.Load(this);
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
                destination = new VaultLoginPageViewModel(vaultViewModel, null); // TODO(r)
                _ = destination.InitAsync();
            }

            // Navigate
            Navigation.Navigate(destination, new EntranceNavigationTransition());
        }

        private async void MainAppHostControl_OnLoaded(object? sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register(this);
            WeakReferenceMessenger.Default.Register<NavigationMessage>(Navigation);

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

        private void Sidebar_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is SidebarItemViewModel itemViewModel)
                NavigateToItem(itemViewModel.VaultViewModel);
        }

        private async void AutoCompleteBox_OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            await ViewModel.SidebarViewModel.SearchViewModel.SubmitQuery((sender as AutoCompleteBox)?.Text ?? string.Empty);
        }

        private void AutoCompleteBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
                return;

            var chosenItem = ViewModel.SidebarViewModel.SidebarItems.FirstOrDefault(x => x.VaultViewModel.VaultModel.VaultName.Equals(((AutoCompleteBox)sender).SelectedItem?.ToString()));
            if (chosenItem is null)
                return;

            Sidebar.SelectedItem = chosenItem;
            NavigateToItem(chosenItem.VaultViewModel);
        }
    }
}