using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents a storage root of a virtual file system.
    /// </summary>
    public interface IVFSRoot : IWrapper<IFolder>, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the display name of the storage.
        /// </summary>
        string StorageName { get; }

        /// <summary>
        /// Gets the fully-qualified name of the file system this storage root is a part of.
        /// </summary>
        string FileSystemName { get; }

        /// <summary>
        /// Gets the instance of <see cref="IHealthStatistics"/> which is used to report health issues of the vault.
        /// </summary>
        IHealthStatistics HealthStatistics { get; }

        /// <summary>
        /// Gets the instance of <see cref="IFileSystemStatistics"/> which is used to report the read-write statistics for this storage root.
        /// </summary>
        IFileSystemStatistics ReadWriteStatistics { get; }
    }
}
