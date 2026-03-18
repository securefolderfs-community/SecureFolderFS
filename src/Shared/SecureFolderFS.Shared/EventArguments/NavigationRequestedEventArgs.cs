using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.EventArguments
{
    /// <summary>
    /// Event arguments for navigation requests.
    /// </summary>
    public abstract class NavigationRequestedEventArgs(IViewable? origin, TaskCompletionSource<bool>? taskCompletion = null) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IViewable"/> instance that invoked the navigation.
        /// </summary>
        public IViewable? Origin { get; } = origin;

        /// <summary>
        /// Gets the optional <see cref="TaskCompletionSource"/> to notify when the navigation operation is finished.
        /// </summary>
        public TaskCompletionSource<bool>? TaskCompletion { get; } = taskCompletion;
    }
}
