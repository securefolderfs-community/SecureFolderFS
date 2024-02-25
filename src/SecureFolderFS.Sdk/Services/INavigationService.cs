using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service used for target-to-target navigation of <see cref="IViewDesignation"/>s.
    /// </summary>
    public interface INavigationService : IDisposable
    {
        /// <summary>
        /// An event that is fired when navigation occurs.
        /// </summary>
        event EventHandler<IViewDesignation?>? NavigationChanged;

        /// <summary>
        /// Gets the value that determines whether this service is initialized and can handle navigation.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the currently navigated-to target.
        /// </summary>
        IViewDesignation? CurrentView { get; }

        /// <summary>
        /// Gets the collection of all previously navigated-to targets.
        /// </summary>
        ICollection<IViewDesignation> Views { get; }

        /// <summary>
        /// Navigates to a given <paramref name="view"/> and updates existing <see cref="CurrentView"/>.
        /// </summary>
        /// <param name="view">The target to navigate to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        Task<bool> NavigateAsync(IViewDesignation view);

        /// <summary>
        /// Tries to navigate to the previous target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true; otherwise false.</returns>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Tries to navigate to the next target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true; otherwise false.</returns>
        Task<bool> GoForwardAsync();
    }
}
