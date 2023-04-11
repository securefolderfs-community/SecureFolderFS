using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.Animations.Transitions;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    internal sealed class AvaloniaNavigationService : BaseNavigationService<ContentNavigationControl>
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
                    if (NavigationControl.CanGoBack)
                    {
                        NavigationControl.GoBack();
                        // TODO: Set current target

                        return true;
                    }

                    return false;
                }

                case NavigationType.Forward:
                    return false;

                default:
                case NavigationType.Detached:
                {
                    if (target is null)
                        return false;

                    return await NavigationControl.NavigateAsync(target, (TransitionBase?)null);
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
