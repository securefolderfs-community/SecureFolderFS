using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.WinUI.Views.Vault;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class VaultNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                VaultLoginPageViewModel => typeof(VaultLoginPage),
                VaultDashboardPageViewModel => typeof(VaultDashboardPage),
                _ => throw new ArgumentNullException(nameof(target))
            };

            var transitionInfo = transition as NavigationTransitionInfo ?? ContentFrame.Content switch
            {
                VaultDashboardPage => new ContinuumNavigationTransitionInfo(), // Dashboard closing animation
                _ => new EntranceNavigationTransitionInfo() // Standard animation
            };

            var result = ContentFrame.Navigate(pageType, target, transitionInfo);
            return Task.FromResult(result);
        }

        public void ClearContent()
        {
            ContentFrame.Content = null;
        }
    }
}
