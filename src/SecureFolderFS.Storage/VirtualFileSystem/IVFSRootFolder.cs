using OwlCore.Storage;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents a storage root of a virtual file system.
    /// </summary>
    public interface IVFSRootFolder : IFolder, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the fully-qualified name of the file system this storage root is a part of.
        /// </summary>
        string FileSystemName { get; }

        /// <summary>
        /// Gets the instance of <see cref="IReadWriteStatistics"/> which is used to report the read-write statistics for this storage root.
        /// </summary>
        IReadWriteStatistics ReadWriteStatistics { get; }
    }
}
