using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class MauiNavigationService : BaseNavigationService
    {
        public int IndexInNavigation { get; private set; }
        
        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(IViewDesignation? target, NavigationType navigationType)
        {
            if (Navigator is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (!await Navigator.GoBackAsync())
                        return false;
                        
                    IndexInNavigation--;
                    CurrentView = Views[IndexInNavigation];
                    
                    return true;
                }
                
                case NavigationType.Forward:
                {
                    if (!await Navigator.GoForwardAsync())
                        return false;
                        
                    IndexInNavigation++;
                    CurrentView = Views[IndexInNavigation];
                    
                    return true;
                }

                default:
                case NavigationType.Chained:
                {
                    if (!await Navigator.NavigateAsync(target))
                        return false;

                    IndexInNavigation++;
                    if (IndexInNavigation >= 0 && IndexInNavigation < Views.Count)
                    {
                        // Remove all elements after the specified index
                        Views.RemoveRange(IndexInNavigation + 1, Views.Count - (IndexInNavigation + 1));
                    }

                    return true;
                }
            }
        }

        internal void SetCurrentViewInternal(IViewDesignation? viewDesignation)
        {
            CurrentView = viewDesignation;
        }
    }
}
