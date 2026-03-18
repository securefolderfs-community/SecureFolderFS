using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for destination navigation requests.
    /// </summary>
    public sealed class DestinationNavigationRequestedEventArgs(IViewable origin, IViewable? destination, TaskCompletionSource<bool>? taskCompletion = null)
        : NavigationRequestedEventArgs(origin, taskCompletion)
    {
        /// <summary>
        /// Gets the destination viewable to navigate to.
        /// </summary>
        public IViewable? Destination { get; } = destination;
    }
}