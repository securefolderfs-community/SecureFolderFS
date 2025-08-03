using OwlCore.Storage;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents a storage root of a virtual file system.
    /// </summary>
    public interface IVFSRoot : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the virtualized storage root folder of the file system.
        /// </summary>
        IFolder VirtualizedRoot { get; }

        /// <summary>
        /// Gets the fully-qualified name of the file system this storage root is a part of.
        /// </summary>
        string FileSystemName { get; }

        /// <summary>
        /// Gets the <see cref="VirtualFileSystemOptions"/> instance for this file system.
        /// </summary>
        VirtualFileSystemOptions Options { get; }
    }
}
