using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Core.Dokany.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public sealed class DokanyOptions : FileSystemOptions
    {
        private string? _mountPoint;

        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPoint { get => _mountPoint; init => _mountPoint = value; }

        internal void SetMountPointInternal(string? value) => _mountPoint = value;

        public static DokanyOptions ToOptions(IDictionary<string, object> options)
        {
            return new()
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? new HealthStatistics(),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? new FileSystemStatistics(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? true,

                // Dokany specific
                MountPoint = (string?)options.Get(nameof(MountPoint))
            };
        }
    }
}
