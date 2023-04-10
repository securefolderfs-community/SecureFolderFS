using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public abstract class BaseNavigationService : INavigationService
    {
        /// <inheritdoc/>
        public INavigationTarget? CurrentTarget { get; protected set; }

        /// <inheritdoc/>
        public ICollection<INavigationTarget> Targets { get; protected set; }

        protected BaseNavigationService()
        {
            Targets = new List<INavigationTarget>();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> NavigateAsync(INavigationTarget target)
        {
            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            // Notify the new target that it's been navigated to
            target.OnNavigatingTo(NavigationType.Detached);

            // Start actual navigation
            var navigationResult = await BeginNavigationAsync(target, NavigationType.Detached);
            if (!navigationResult)
                return false;

            // Update current target
            CurrentTarget = target;

            // Add new target
            if (!Targets.Contains(target))
                Targets.Add(target);

            return true;
        }

        /// <inheritdoc/>
        public virtual Task<bool> GoBackAsync()
        {
            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            return BeginNavigationAsync(null, NavigationType.Backward);
        }

        /// <inheritdoc/>
        public virtual Task<bool> GoForwardAsync()
        {
            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            return BeginNavigationAsync(null, NavigationType.Forward);
        }

        /// <summary>
        /// Starts the navigation routine.
        /// </summary>
        /// <param name="target">The target to navigate to.</param>
        /// <param name="navigationType">The type of navigation to perform.</param>
        /// <remarks>
        /// Parameter <paramref name="target"/> may be null when the parameter <paramref name="navigationType"/>
        /// is set to <see cref="NavigationType.Backward"/> or <see cref="NavigationType.Forward"/>.
        /// In such case, it is the implementor's responsibility to update the <see cref="CurrentTarget"/> property
        /// and notify the <see cref="INavigationTarget"/> that it's being navigated to.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        protected abstract Task<bool> BeginNavigationAsync(INavigationTarget? target, NavigationType navigationType);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Targets.DisposeCollection();
            Targets.Clear();

            CurrentTarget = null;
        }
    }
}
