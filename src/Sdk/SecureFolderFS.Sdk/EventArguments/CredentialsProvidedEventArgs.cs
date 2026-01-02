using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for authentication provided events.
    /// </summary>
    public sealed class CredentialsProvidedEventArgs(IKeyBytes authentication, TaskCompletionSource? taskCompletion) : EventArgs
    {
        /// <summary>
        /// Gets the authentication that was provided.
        /// </summary>
        public IKeyBytes Authentication { get; } = authentication;

        /// <summary>
        /// Gets the optional <see cref="TaskCompletionSource"/> to notify when the unlock operation is finished.
        /// </summary>
        public TaskCompletionSource? TaskCompletion { get; } = taskCompletion;
    }
}
