using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Enums;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <summary>
    /// Represents a virtual encrypting file system.
    /// </summary>
    public interface IVirtualFileSystem : IAsyncDisposable
    {
        /// <summary>
        /// Gets the root directory of this virtual file system instance.
        /// </summary>
        /// <remarks>The folder will become invalid if the virtual file system is destroyed.</remarks>
        IFolder RootFolder { get; }

        /// <summary>
        /// Gets the value indicating whether this virtual file system instance is accessible and running.
        /// </summary>
        bool IsOperational { get; }

        /// <summary>
        /// Tries to close this virtual file system instance invalidating <see cref="RootFolder"/> if successful.
        /// </summary>
        /// <param name="closeMethod">Determines the method to use when closing the file system.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns true if the file system was closed successfully; otherwise false.</returns>
        Task<bool> CloseAsync(FileSystemCloseMethod closeMethod);
    }
}
