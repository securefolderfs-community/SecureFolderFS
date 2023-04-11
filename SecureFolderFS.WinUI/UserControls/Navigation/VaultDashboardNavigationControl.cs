using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.WinUI.Views.Vault;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class VaultDashboardNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                VaultOverviewPageViewModel => typeof(VaultOverviewPage),
                VaultPropertiesPageViewModel => typeof(VaultPropertiesPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            var transitionInfo = transition as NavigationTransitionInfo ?? new SlideNavigationTransitionInfo();
            var result = ContentFrame.Navigate(pageType, target, transitionInfo);

            return Task.FromResult(result);
        }
    }
}
