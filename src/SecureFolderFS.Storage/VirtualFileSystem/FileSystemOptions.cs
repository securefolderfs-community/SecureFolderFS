using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;

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
        /// Gets or sets whether to use a read-only file system.
        /// </summary>
        public bool IsReadOnly { get; init; }

        /// <summary>
        /// Gets or sets whether to enable caching for decrypted content chunks.
        /// </summary>
        public bool IsCachingChunks { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to enable caching for ciphertext and plaintext names.
        /// </summary>
        public bool IsCachingFileNames { get; init; } = true;

        public static FileSystemOptions ToOptions(
            IDictionary<string, object> options,
            Func<IHealthStatistics> healthStatistics,
            Func<IFileSystemStatistics> fileSystemStatistics)
        {
            return new()
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? healthStatistics.Invoke(),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? fileSystemStatistics.Invoke(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? true,
            };
        }

        public virtual T? GetOption<T>(string name)
        {
            return (T)(object)(name switch
            {
                nameof(VolumeName) => VolumeName,
                nameof(HealthStatistics) => HealthStatistics,
                nameof(FileSystemStatistics) => FileSystemStatistics,
                nameof(IsReadOnly) => IsReadOnly,
                nameof(IsCachingChunks) => IsCachingChunks,
                nameof(IsCachingFileNames) => IsCachingFileNames,
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            });
        }
    }
}
