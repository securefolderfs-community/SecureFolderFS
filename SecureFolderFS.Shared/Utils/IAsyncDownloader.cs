using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Manages and downloads requested resource of type <typeparamref name="TDownloaded"/>.
    /// </summary>
    /// <typeparam name="TDownloaded">The type of resource to be downloaded</typeparam>
    public interface IAsyncDownloader<TDownloaded> : IDisposable
    {
        /// <summary>
        /// Downloads a resource asynchronously.
        /// </summary>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult{T}"/> of <typeparamref name="TDownloaded"/> that represents the downloaded resource.</returns>
        Task<IResult<TDownloaded>> DownloadAsync(IProgress<double> progress, CancellationToken cancellationToken = default);
    }
}
