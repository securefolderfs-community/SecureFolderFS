using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public abstract class BaseNavigationService : INavigationService, INavigationControlContract
    {
        /// <inheritdoc/>
        public IViewDesignation? CurrentView { get; protected set; }

        /// <inheritdoc/>
        public ICollection<IViewDesignation> Views { get; protected set; }

        /// <inheritdoc/>
        public INavigationControl? NavigationControl { get; set; }

        /// <inheritdoc/>
        public event EventHandler<IViewDesignation?>? NavigationChanged;

        /// <summary>
        /// Gets the value that determines whether this service is initialized and can handle navigation.
        /// </summary>
        public virtual bool IsInitialized => NavigationControl is not null;

        protected BaseNavigationService()
        {
            Views = new List<IViewDesignation>();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> NavigateAsync(IViewDesignation view)
        {
            if (!IsInitialized)
                return false;

            // Notify the current target that it's being navigated from
            CurrentView?.OnDisappearing();

            // Notify the new target that it's been navigated to
            view.OnAppearing();
            
            // Update current target
            CurrentView = view;

            // Add new target
            if (!Views.Contains(view))
                Views.Add(view);

            // Start actual navigation
            var navigationResult = await BeginNavigationAsync(view, NavigationType.Chained);
            if (!navigationResult)
                return false;

            // Notify that navigation has occurred
            NavigationChanged?.Invoke(this, view);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> GoBackAsync()
        {
            if (!IsInitialized)
                return false;

            // Notify the current target that it's being navigated from
            CurrentView?.OnDisappearing();

            var navigationResult = await BeginNavigationAsync(null, NavigationType.Backward);
            if (navigationResult)
            {
                // Notify that navigation has occurred
                NavigationChanged?.Invoke(this, CurrentView);
            }

            return navigationResult;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> GoForwardAsync()
        {
            if (!IsInitialized)
                return false;

            // Notify the current target that it's being navigated from
            CurrentView?.OnDisappearing();

            var navigationResult = await BeginNavigationAsync(null, NavigationType.Forward);
            if (navigationResult)
            {
                // Notify that navigation has occurred
                NavigationChanged?.Invoke(this, CurrentView);
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
        /// In such case, it is the implementor's responsibility to update the <see cref="CurrentView"/> property
        /// and notify the <see cref="IViewDesignation"/> that it's being navigated to.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        protected abstract Task<bool> BeginNavigationAsync(IViewDesignation? target, NavigationType navigationType);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            CurrentView = null;
            NavigationControl?.Dispose();

            Views.DisposeElements();
            Views.Clear();
        }
    }
}
