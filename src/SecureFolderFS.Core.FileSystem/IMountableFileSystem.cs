using SecureFolderFS.Core.FileSystem.AppModels;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <summary>
    /// Represents a file system that can be mounted for use.
    /// </summary>
    public interface IMountableFileSystem
    {
        /// <summary>
        /// Mounts the virtual file system using <paramref name="mountOptions"/>.
        /// </summary>
        /// <param name="mountOptions">Options specifying mount operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns an instance of <see cref="IVirtualFileSystem"/> of the mounted file system; otherwise false.</returns>
        Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default);
    }
}
