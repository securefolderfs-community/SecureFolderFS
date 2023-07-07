using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Statistics;

namespace SecureFolderFS.Core.Models
{
    /// <summary>
    /// Specifies file system related options to use.
    /// </summary>
    public sealed class FileSystemOptions
    {
        /// <summary>
        /// Gets the type of the virtual file system adapter to be used.
        /// </summary>
        public required FileSystemAdapterType AdapterType { get; init; }

        /// <summary>
        /// Gets the instance file system statistics which are reported by the underlying virtual file system.
        /// </summary>
        public IFileSystemStatistics? FileSystemStatistics { get; init; }

        /// <summary>
        /// Gets the instance file system health statistics which are reported by the underlying virtual file system.
        /// </summary>
        public IFileSystemHealthStatistics? HealthStatistics { get; init; }

        /// <summary>
        /// Gets the data caching strategy for decrypted content chunks.
        /// </summary>
        public ChunkCachingStrategy ChunkCachingStrategy { get; init; } = ChunkCachingStrategy.RandomAccessMemoryCache;

        /// <summary>
        /// Gets the caching strategy for ciphertext and cleartext names.
        /// </summary>
        public FileNameCachingStrategy FileNameCachingStrategy { get; init; } = FileNameCachingStrategy.RandomAccessMemoryCache;

        /// <summary>
        /// Gets the caching strategy for DirectoryIDs.
        /// </summary>
        public DirectoryIdCachingStrategy DirectoryIdCachingStrategy { get; init; } = DirectoryIdCachingStrategy.RandomAccessMemoryCache;
    }
}
