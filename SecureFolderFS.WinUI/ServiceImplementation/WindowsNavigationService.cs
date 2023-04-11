using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.WinUI.UserControls.Navigation;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class WindowsNavigationService : BaseNavigationService<FrameNavigationControl>
    {
        /// <inheritdoc/>
        public override Task<bool> NavigateAsync(INavigationTarget target)
        {
            if (NavigationControl is null)
                return Task.FromResult(false);

            return base.NavigateAsync(target);
        }

        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(INavigationTarget? target, NavigationType navigationType)
        {
            if (NavigationControl is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (NavigationControl.ContentFrame.CanGoBack)
                    {
                        NavigationControl.ContentFrame.GoBack();
                        // TODO: Set current target

                        return true;
                    }

                    return false;
                }

                case NavigationType.Forward:
                {
                    if (NavigationControl.ContentFrame.CanGoForward)
                    {
                        NavigationControl.ContentFrame.GoForward();
                        // TODO: Set current target

                        return true;
                    }

                    return false;
                }

                default:
                case NavigationType.Detached:
                {
                    if (target is null)
                        return false;

                    return await NavigationControl.NavigateAsync(target, (NavigationTransitionInfo?)null);
                }
            }
        }
    }
}
