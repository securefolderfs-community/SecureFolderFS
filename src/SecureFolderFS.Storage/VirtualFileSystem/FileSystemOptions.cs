namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Specifies file system related options to use.
    /// </summary>
    public class FileSystemOptions
    {
        /// <summary>
        /// Gets or sets the name to use for the volume.
        /// </summary>
        public required string VolumeName { get; init; }

        /// <summary>
        /// Gets or sets the instance file system health statistics which are reported by the underlying virtual file system.
        /// </summary>
        public required IHealthStatistics HealthStatistics { get; init; }

        /// <summary>
        /// Gets or sets the instance file system statistics which are reported by the underlying virtual file system.
        /// </summary>
        public required IFileSystemStatistics FileSystemStatistics { get; init; }

        /// <summary>
        /// Gets or sets whether to enable caching for decrypted content chunks.
        /// </summary>
        public bool EnableChunkCache { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to enable caching for ciphertext and plaintext names.
        /// </summary>
        public bool EnableFileNameCache { get; init; } = true;
    }
}
