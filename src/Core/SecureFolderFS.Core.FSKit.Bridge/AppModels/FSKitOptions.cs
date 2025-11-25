using System;
using System.Collections.Generic;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FSKit.Bridge.AppModels
{
    public sealed class FSKitOptions : VirtualFileSystemOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPoint { get; private set; }

        public void DangerousSetMountPoint(string mountPoint)
        {
            MountPoint = mountPoint;
        }

        public static FSKitOptions ToOptions(IDictionary<string, object> options)
        {
            return new FSKitOptions
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? new HealthStatistics(),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? new FileSystemStatistics(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? true,
                IsCachingDirectoryIds = (bool?)options.Get(nameof(IsCachingDirectoryIds)) ?? true,
                RecycleBinSize = (long?)options.Get(nameof(RecycleBinSize)) ?? 0L,

                // FSKit specific
                MountPoint = (string?)options.Get(nameof(MountPoint))
            };
        }
    }
}

