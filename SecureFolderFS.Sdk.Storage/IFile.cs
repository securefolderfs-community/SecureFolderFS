using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Represents a file on the file system.
    /// </summary>
    public interface IFile : IBaseStorage
    {
        /// <summary>
        /// Gets the extension of the file.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Opens and returns a stream to the file.
        /// </summary>
        /// <param name="access">The file access to open the file with.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a <see cref="Stream"/>, otherwise null.</returns>
        Task<Stream?> OpenStreamAsync(FileAccess access);

        /// <inheritdoc cref="OpenStreamAsync(System.IO.FileAccess)"/>
        /// <param name="access">The file access to open the file with.</param>
        /// <param name="share">The file share to open the file with.</param>
        /// <remarks>The <paramref name="share"/> may not be supported on some platforms.</remarks>
        Task<Stream?> OpenStreamAsync(FileAccess access, FileShare share);

        /// <summary>
        /// Retrieves the thumbnail for the file, adjusted by <paramref name="requestedSize"/>.
        /// </summary>
        /// <param name="requestedSize">The size to get the thumbnail with.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task<Stream> GetThumbnailStreamAsync(uint requestedSize); // TODO: Return IImage nullable
    }
}
