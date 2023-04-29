using System.Linq;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.WinUI.UserControls.Navigation;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class WindowsNavigationService : BaseNavigationService<FrameNavigationControl>
    {
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

                        var targetType = NavigationControl.TypeBinding.GetByKeyOrValue(NavigationControl.ContentFrame.Content.GetType());
                        var backTarget = Targets.FirstOrDefault(x => x.GetType() == targetType);
                        if (backTarget is not null)
                            CurrentTarget = backTarget;

                        return true;
                    }

                    return false;
                }

                case NavigationType.Forward:
                {
                    if (NavigationControl.ContentFrame.CanGoForward)
                    {
                        NavigationControl.ContentFrame.GoForward();

                        var targetType = NavigationControl.TypeBinding.GetByKeyOrValue(NavigationControl.ContentFrame.Content.GetType());
                        var forwardTarget = Targets.FirstOrDefault(x => x.GetType() == targetType);
                        if (forwardTarget is not null)
                            CurrentTarget = forwardTarget;

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
