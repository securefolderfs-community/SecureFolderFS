using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultDashboardNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultOverviewPageViewModel), typeof(VaultOverviewPage) },
            { typeof(VaultPropertiesPageViewModel), typeof(VaultPropertiesPage) }
        };

        /// <inheritdoc/>
        protected override Task<bool> NavigateContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            transition ??= new EntranceNavigationTransition();
            return SetContentAsync(pageType, parameter, transition);
        }
    }
}