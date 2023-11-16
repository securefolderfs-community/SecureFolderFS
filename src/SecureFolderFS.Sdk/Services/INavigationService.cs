﻿using SecureFolderFS.Sdk.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A model used for target-to-target navigation of <see cref="INavigationTarget"/>s.
    /// </summary>
    public interface INavigationService : IDisposable
    {
        /// <summary>
        /// An event that is fired when navigation occurs.
        /// </summary>
        event EventHandler<INavigationTarget?>? NavigationChanged;

        /// <summary>
        /// Gets the value that determines whether this service is initialized and can handle navigation.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the currently navigated-to target.
        /// </summary>
        INavigationTarget? CurrentTarget { get; }

        /// <summary>
        /// Gets the collection of all previously navigated-to targets.
        /// </summary>
        ICollection<INavigationTarget> Targets { get; }

        /// <summary>
        /// Navigates to a given <paramref name="target"/> and updates existing <see cref="CurrentTarget"/>.
        /// </summary>
        /// <param name="target">The target to navigate to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        Task<bool> NavigateAsync(INavigationTarget target);

        /// <summary>
        /// Tries to navigate to the previous target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true, otherwise false.</returns>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Tries to navigate to the next target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true, otherwise false.</returns>
        Task<bool> GoForwardAsync();
    }

    /// <summary>
    /// Represents a target which can be navigated to.
    /// </summary>
    public interface INavigationTarget
    {
        /// <summary>
        /// Notifies the implementation that the target is being navigated to.
        /// </summary>
        /// <param name="navigationType">Informs how the navigation was triggered.</param>
        void OnNavigatingTo(NavigationType navigationType);

        /// <summary>
        /// Notifies the implementation that the target is being navigated from.
        /// </summary>
        void OnNavigatingFrom();
    }
}
