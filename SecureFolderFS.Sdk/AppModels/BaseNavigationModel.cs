using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="INavigationModel{T}"/>
    public abstract class BaseNavigationModel<T> : INavigationModel<T>
        where T : notnull
    {
        protected readonly Dictionary<T, INavigationTarget> navigationTargets;

        protected BaseNavigationModel()
        {
            navigationTargets = new();
        }

        /// <inheritdoc/>
        public INavigationTarget? CurrentTarget { get; protected set; }

        /// <inheritdoc/>
        public virtual INavigationTarget? TryGet(T identifier)
        {
            navigationTargets.TryGetValue(identifier, out var target);
            return target;
        }

        /// <inheritdoc/>
        public virtual void Remove(INavigationTarget target)
        {
            if (navigationTargets.TryFirstOrDefault(x => x.Value.Equals(target), out var itemToRemove))
                return;

            navigationTargets.Remove(itemToRemove.Key);
        }

        /// <inheritdoc/>
        public virtual Task<bool> GoBackAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public virtual Task<bool> GoForwardAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public virtual async Task<T> NavigateAsync(INavigationTarget target)
        {
            // Notify the current target that it's being navigated from
            CurrentTarget?.OnNavigatingFrom();

            // Start actual navigation
            var identifier = await BeginNavigationAsync(target, NavigationType.Detached);

            // Notify the new target that it's been navigated to
            target.OnNavigatingTo(NavigationType.Detached);

            // Update targets
            navigationTargets[identifier] = target;
            CurrentTarget = target;

            return identifier;
        }

        /// <summary>
        /// Starts the navigation routine.
        /// </summary>
        /// <param name="target">The target to navigate to.</param>
        /// <param name="navigationType">The type of navigation to perform.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an identifier of type <typeparamref name="T"/> that can be associated with <paramref name="target"/>.</returns>
        protected abstract Task<T> BeginNavigationAsync(INavigationTarget target, NavigationType navigationType);
    }
}
