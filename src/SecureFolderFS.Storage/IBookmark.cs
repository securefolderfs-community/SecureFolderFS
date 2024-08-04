using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage
{
    public interface IBookmark : IStorable
    {
        string? BookmarkId { get; }

        /// <summary>
        /// Adds application access to a bookmarked file system resource.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task AddBookmarkAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes application access from a bookmarked file system resource.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RemoveBookmarkAsync(CancellationToken cancellationToken = default);
    }
}
