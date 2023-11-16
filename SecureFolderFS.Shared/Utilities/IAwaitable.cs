using System.Runtime.CompilerServices;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents an awaitable action.
    /// </summary>
    public interface IAwaitable : INotifyCompletion
    {
        /// <summary>
        /// Gets whether this <see cref="IAwaitable"/> has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="IAwaitable"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        IAwaitable GetAwaiter();

        /// <summary>
        /// Ends the await on the completed <see cref="IAwaitable"/>.
        /// </summary>
        void GetResult();
    }
}
