using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultDashboardNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                VaultOverviewPageViewModel => typeof(VaultOverviewPage),
                VaultPropertiesPageViewModel => typeof(VaultPropertiesPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            var transitionInfo = transition as TransitionBase ?? new EntranceNavigationTransition();
            await Navigate(pageType, target, transitionInfo);

            return true;
        }
    }
}