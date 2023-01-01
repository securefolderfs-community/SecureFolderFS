using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.ExtendableStorage
{
    /// <summary>
    /// Extends existing <see cref="IFile"/> interface with additional properties.
    /// </summary>
    public interface IFileExtended : IFile
    {
        /// <param name="share">The file sharing flags that specify access other processes have to the file.</param>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        Task<Stream> OpenStreamAsync(FileAccess access, FileShare share, CancellationToken cancellationToken = default);
    }
}
