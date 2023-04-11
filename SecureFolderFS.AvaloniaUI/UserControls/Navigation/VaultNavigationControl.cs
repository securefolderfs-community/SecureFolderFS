using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                VaultLoginPageViewModel => typeof(VaultLoginPage),
                VaultDashboardPageViewModel => typeof(VaultDashboardPage),
                _ => throw new ArgumentNullException(nameof(target))
            };

            var transitionInfo = transition as TransitionBase ?? CurrentContent switch
            {
                // (TODO: Add dashboard closing animation here - infer from the current content) // Dashboard closing animation
                _ => new EntranceNavigationTransition() // Standard animation
            };
            
            await Navigate(pageType, target, transitionInfo);
            return true;
        }
        
        public void ClearContent()
        {
            CurrentContent = null;
        }
    }
}