using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Manages the download process of a resource.
    /// </summary>
    public interface IAsyncDownloader : IDisposable
    {
        /// <summary>
        /// Downloads a resource asynchronously.
        /// </summary>
        /// <param name="progress">The progress of the operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation.</returns>
        Task<IResult> DownloadAsync(IProgress<double> progress, CancellationToken cancellationToken = default);
    }
}
