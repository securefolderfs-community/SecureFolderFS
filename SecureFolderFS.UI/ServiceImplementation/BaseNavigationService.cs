using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.UI.Controls;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public abstract class BaseNavigationService<TNavigation> : INavigationService
        where TNavigation : class, INavigationControl
    {
        /// <summary>
        /// Gets or sets the control used for navigation.
        /// </summary>
        public TNavigation? NavigationControl { get; set; }

        /// <inheritdoc/>
        public event EventHandler<INavigationTarget?>? NavigationChanged;

        /// <inheritdoc/>
        public virtual bool IsInitialized => NavigationControl is not null;

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
            if (!IsInitialized)
                return false;

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
            {
                Targets.Add(target);

                // Initialize if the target supports IAsyncInitialize
                if (target is IAsyncInitialize asyncInitialize)
                    _ = asyncInitialize.InitAsync();
            }

            // Notify that navigation has occurred
            NavigationChanged?.Invoke(this, target);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> GoBackAsync()
        {
            if (!IsInitialized)
                return false;

            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            var navigationResult = await BeginNavigationAsync(null, NavigationType.Backward);
            if (navigationResult)
            {
                // Notify that navigation has occurred
                NavigationChanged?.Invoke(this, CurrentTarget);
            }

            return navigationResult;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> GoForwardAsync()
        {
            if (!IsInitialized)
                return false;

            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            var navigationResult = await BeginNavigationAsync(null, NavigationType.Forward);
            if (navigationResult)
            {
                // Notify that navigation has occurred
                NavigationChanged?.Invoke(this, CurrentTarget);
            }

            return navigationResult;
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
            CurrentTarget = null;
            NavigationControl?.Dispose();

            Targets.DisposeCollection();
            Targets.Clear();
        }
    }
}
