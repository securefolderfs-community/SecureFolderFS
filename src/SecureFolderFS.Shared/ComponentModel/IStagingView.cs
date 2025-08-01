using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a staging view interface that enables staged or multi-step operations.
    /// Provides functionality to attempt continuation or cancellation of the current operation asynchronously.
    /// </summary>
    public interface IStagingView : IViewDesignation
    {
        /// <summary>
        /// Attempts to continue the current operation asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result of the task is an <see cref="IResult"/> indicating whether the continuation was successful or an error occurred.</returns>
        Task<IResult> TryContinueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to cancel the current operation asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result of the task is an <see cref="IResult"/> indicating whether the cancellation was successful or an error occurred.</returns>
        Task<IResult> TryCancelAsync(CancellationToken cancellationToken);
    }
}
