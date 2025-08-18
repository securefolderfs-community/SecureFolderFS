using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for wizard navigation requests.
    /// </summary>
    public sealed class DestinationNavigationRequestedEventArgs(IViewable? destination, IViewable origin)
        : NavigationRequestedEventArgs(origin)
    {
        /// <summary>
        /// Gets the destination viewable to navigate to.
        /// </summary>
        public IViewable? Destination { get; } = destination;
    }
}