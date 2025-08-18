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
            if (Navigator is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (Navigator is not FrameNavigationControl frameNavigation)
                        return false;

                    // Navigate back
                    if (!await Navigator.GoBackAsync())
                        return false;

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
                    if (Navigator is not FrameNavigationControl frameNavigation)
                        return false;

                    // Navigate forward
                    if (!await Navigator.GoForwardAsync())
                        return false;

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

                    return await Navigator.NavigateAsync(view);
                }
            }
        }
    }
}
