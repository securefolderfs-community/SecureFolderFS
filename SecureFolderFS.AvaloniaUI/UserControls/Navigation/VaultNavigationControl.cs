using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultLoginPageViewModel), typeof(VaultLoginPage) },
            { typeof(VaultDashboardPageViewModel), typeof(VaultDashboardPage) }
        };

        /// <inheritdoc/>
        protected override Task<bool> NavigateContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            transition ??= CurrentContent switch
            {
                // (TODO: Add dashboard closing animation here - infer from the current content) // Dashboard closing animation
                _ => new EntranceNavigationTransition() // Standard animation
            };

            return SetContentAsync(pageType, parameter, transition);
        }

        public void ClearContent()
        {
            CurrentContent = null;
        }
    }
}