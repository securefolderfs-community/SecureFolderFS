using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service used for target-to-target navigation of <see cref="IViewDesignation"/>s.
    /// </summary>
    public interface INavigationService : INavigator, IDisposable
    {
        /// <summary>
        /// An event that is fired when navigation occurs.
        /// </summary>
        event EventHandler<IViewDesignation?>? NavigationChanged;

        /// <summary>
        /// Gets the currently navigated-to target.
        /// </summary>
        IViewDesignation? CurrentView { get; }

        /// <summary>
        /// Gets the collection of all previously navigated-to targets.
        /// </summary>
        List<IViewDesignation> Views { get; }
    }
}
