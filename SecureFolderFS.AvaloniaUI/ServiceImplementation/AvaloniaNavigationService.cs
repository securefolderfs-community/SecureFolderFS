using Avalonia.Threading;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using System.Linq;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class AvaloniaNavigationService : BaseNavigationService
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
                    if (NavigationControl is not FrameNavigationControl frameNavigation)
                        return false;

                    if (!frameNavigation.ContentFrame.CanGoBack)
                        return false;

                    // Navigate back
                    frameNavigation.ContentFrame.GoBack();

                    var contentType = frameNavigation.ContentFrame.Content?.GetType();
                    if (contentType is null)
                        return false;

                    var targetType = frameNavigation.TypeBinding.GetByKeyOrValue(contentType);
                    var backTarget = Targets.FirstOrDefault(x => x.GetType() == targetType);
                    if (backTarget is not null)
                        CurrentTarget = backTarget;

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
                    var forwardTarget = Targets.FirstOrDefault(x => x.GetType() == targetType);
                    if (forwardTarget is not null)
                        CurrentTarget = forwardTarget;

                    return true;
                }

                default:
                case NavigationType.Detached:
                {
                    if (target is null)
                        return false;

                    return await Dispatcher.UIThread.InvokeAsync(() => NavigationControl.NavigateAsync(target, (NavigationTransition?)null));
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            NavigationControl?.Dispose();
            base.Dispose();
        }
    }
}
