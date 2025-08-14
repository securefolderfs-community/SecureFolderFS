using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Provides a contract for managing and connecting to a remote resource of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the remote resource.</typeparam>
    public interface IRemoteResource<T> : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Establishes an asynchronous connection and retrieves the remote resource of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result is the remote resource of type <typeparamref name="T"/>.</returns>
        Task<T> ConnectAsync(CancellationToken cancellationToken = default);
    }
}
