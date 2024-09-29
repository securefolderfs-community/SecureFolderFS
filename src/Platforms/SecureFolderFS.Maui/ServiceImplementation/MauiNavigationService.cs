using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class MauiNavigationService : BaseNavigationService
    {
        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(IViewDesignation? target, NavigationType navigationType)
        {
            if (NavigationControl is null)
                return false;

            _ = navigationType;
            return await NavigationControl.NavigateAsync<IViewDesignation, string>(target);
        }

        internal void SetCurrentViewInternal(IViewDesignation? viewDesignation)
        {
            CurrentView = viewDesignation;
        }
    }
}
