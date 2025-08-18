using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Shared.EventArguments
{
    /// <summary>
    /// Event arguments for navigation requests.
    /// </summary>
    public abstract class NavigationRequestedEventArgs(IViewable? origin) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IViewable"/> instance that invoked the navigation.
        /// </summary>
        public IViewable? Origin { get; } = origin;
    }
}
