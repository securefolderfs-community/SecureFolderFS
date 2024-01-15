using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.UserControls.Navigation;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public sealed class UnoNavigationService : BaseNavigationService
    {
        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(IViewDesignation? view, NavigationType navigationType)
        {
            if (NavigationControl is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (NavigationControl is not FrameNavigationControl frameNavigation)
                        return false;

                    if (!frameNavigation.ContentFrame.CanGoBack) 
                        return false;

                    // Navigate back
                    frameNavigation.ContentFrame.GoBack();

                    var contentType = frameNavigation.Content?.GetType();
                    if (contentType is null)
                        return false;

                    var targetType = frameNavigation.TypeBinding.GetByKeyOrValue(contentType);
                    var backTarget = Views.FirstOrDefault(x => x.GetType() == targetType);
                    if (backTarget is not null)
                        CurrentView = backTarget;

                    return true;
                }

                case NavigationType.Forward:
                {
                    if (NavigationControl is not FrameNavigationControl frameNavigation)
                        return false;

                    if (!frameNavigation.ContentFrame.CanGoForward)
                        return false;

                    // Navigate forward
                    frameNavigation.ContentFrame.GoForward();

                    var contentType = frameNavigation.ContentFrame.Content?.GetType();
                    if (contentType is null)
                        return false;

                    var targetType = frameNavigation.TypeBinding.GetByKeyOrValue(contentType);
                    var forwardTarget = Views.FirstOrDefault(x => x.GetType() == targetType);
                    if (forwardTarget is not null)
                        CurrentView = forwardTarget;

                    return true;
                }

                default:
                case NavigationType.Chained:
                {
                    if (view is null)
                        return false;

                    return await NavigationControl.NavigateAsync(view, (NavigationTransitionInfo?)null);
                }
            }
        }
    }
}
