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
        /// Gets or sets the type of the virtual file system adapter ID to be used.
        /// </summary>
        public required string FileSystemId { get; init; }

        /// <summary>
        /// Gets or sets the name to use for the volume. If no value is provided, the name of the vault folder is used.
        /// </summary>
        public string? VolumeName { get; init; }

        /// <summary>
        /// Gets or sets the instance file system statistics which are reported by the underlying virtual file system.
        /// </summary>
        public IFileSystemStatistics? FileSystemStatistics { get; init; }

        /// <summary>
        /// Gets or sets the instance file system health statistics which are reported by the underlying virtual file system.
        /// </summary>
        public IFileSystemHealthStatistics? HealthStatistics { get; init; }

        /// <summary>
        /// Gets or sets the data caching strategy for decrypted content chunks.
        /// </summary>
        public ChunkCachingStrategy ChunkCachingStrategy { get; init; } = ChunkCachingStrategy.RandomAccessMemoryCache;

        /// <summary>
        /// Gets or sets the caching strategy for ciphertext and cleartext names.
        /// </summary>
        public FileNameCachingStrategy FileNameCachingStrategy { get; init; } = FileNameCachingStrategy.RandomAccessMemoryCache;
    }
}
