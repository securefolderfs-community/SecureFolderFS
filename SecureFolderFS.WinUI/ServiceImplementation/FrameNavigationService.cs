using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public abstract class FrameNavigationService : BaseNavigationService
    {
        internal Frame? Frame { get; set; }

        /// <inheritdoc/>
        public override Task<bool> NavigateAsync(INavigationTarget target)
        {
            if (Frame is null)
                return Task.FromResult(false);

            return base.NavigateAsync(target);
        }

        /// <inheritdoc/>
        protected override Task<bool> BeginNavigationAsync(INavigationTarget? target, NavigationType navigationType)
        {
            if (Frame is null)
                return Task.FromResult(false);

            switch (navigationType)
            {
                case NavigationType.Backward:
                {
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        // TODO: Set current target

                        Task.FromResult(true);
                    }

                    return Task.FromResult(false);
                }

                case NavigationType.Forward:
                {
                    if (Frame.CanGoForward)
                    {
                        Frame.GoForward();
                        // TODO: Set current target

                        Task.FromResult(true);
                    }

                    return Task.FromResult(false);
                }

                default:
                case NavigationType.Detached:
                {
                    if (target is null)
                        return Task.FromResult(false);

                    var navigationResult = NavigateFrame(Frame, target);
                    return Task.FromResult(navigationResult);
                }
            }
        }

        /// <summary>
        /// Finds appropriate page type based on <paramref name="target"/> and navigates to it.
        /// </summary>
        /// <param name="frame">The frame to use for navigation.</param>
        /// <param name="target">The target to navigate to.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        protected abstract bool NavigateFrame(Frame frame, INavigationTarget target);
    }
}
