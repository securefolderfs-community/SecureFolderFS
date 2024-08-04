using SecureFolderFS.Shared.EventArguments;
using System;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a view which can request to be navigated away from.
    /// </summary>
    public interface INavigatable : IViewable
    {
        /// <summary>
        /// An event that is fired when navigation is requested.
        /// </summary>
        event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
    }
}
