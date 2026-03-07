using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.FileShareOptions
{
    /// <summary>
    /// Represents a file that can be opened with a specified sharing mode.
    /// </summary>
    public interface IFileOpenShare : IFile
    {
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        /// <param name="shareMode">The <see cref="FileShare"/> value that informs what sharing permissions between consumers should be applied.</param>
        Task<Stream> OpenStreamAsync(FileAccess accessMode, FileShare shareMode, CancellationToken cancellationToken = default);
    }
}
