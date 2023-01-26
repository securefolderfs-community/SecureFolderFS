using System;
using FluentAvalonia.UI.Media.Animation;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    internal sealed class VaultDashboardNavigationControl : NavigationControl
    {
        public override void Receive(BackNavigationRequestedMessage message)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
        {
            // TODO: Cache navigation

            var pageType = viewModel switch
            {
                VaultOverviewPageViewModel => typeof(VaultOverviewPage),
                VaultPropertiesPageViewModel => typeof(VaultPropertiesPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, new SlideNavigationTransitionInfo());
        }
    }
}