using System;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    internal sealed class VaultDashboardNavigationControl : NavigationControl
    {
        public override void Receive(BackNavigationRequestedMessage message)
        {
            if (CanGoBack)
                GoBack();
        }

        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransition? transition)
        {
            // TODO: Cache navigation

            var pageType = viewModel switch
            {
                VaultOverviewPageViewModel => typeof(VaultOverviewPage),
                VaultPropertiesPageViewModel => typeof(VaultPropertiesPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            Navigate(pageType, viewModel, new EntranceNavigationTransition());
        }
    }
}