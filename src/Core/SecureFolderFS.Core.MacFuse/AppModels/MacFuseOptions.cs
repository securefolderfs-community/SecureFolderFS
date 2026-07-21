using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse.AppModels
{
    /// <inheritdoc cref="VirtualFileSystemOptions"/>
    public sealed class MacFuseOptions : VirtualFileSystemOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPoint { get; init; }

        /// <summary>
        /// Gets whether the root user should have access to the filesystem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requires the <i>allow_other</i> macFUSE tunable to be enabled (sysctl vfs.generic.macfuse.tunables.allow_other=1).
        /// </para>
        /// <para>
        /// This option is mutually exclusive with <see cref="AllowOtherUserAccess"/>.
        /// </para>
        /// </remarks>
        public bool AllowRootUserAccess { get; init; }

        /// <summary>
        /// Gets whether other users, including root, should have access to the filesystem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requires the <i>allow_other</i> macFUSE tunable to be enabled (sysctl vfs.generic.macfuse.tunables.allow_other=1).
        /// </para>
        /// <para>
        /// This option is mutually exclusive with <see cref="AllowRootUserAccess"/>.
        /// </para>
        /// </remarks>
        public bool AllowOtherUserAccess { get; init; }

        /// <summary>
        /// Gets whether to print debugging information to the console.
        /// </summary>
        public bool PrintDebugInformation { get; init; }

        /// <inheritdoc/>
        public override string? GetDescription()
        {
            return MountPoint;
        }

        public static MacFuseOptions ToOptions(IDictionary<string, object> options)
        {
            return new()
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? new HealthStatistics(),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? new FileSystemStatistics(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? false,
                IsCachingDirectoryIds = (bool?)options.Get(nameof(IsCachingDirectoryIds)) ?? true,
                RecycleBinSize = (long?)options.Get(nameof(RecycleBinSize)) ?? 0L,
                ShorteningThreshold = (int?)options.Get(nameof(ShorteningThreshold)) ?? 0,

                // macFUSE specific
                MountPoint = (string?)options.Get(nameof(MountPoint)),
                AllowRootUserAccess = (bool?)options.Get(nameof(AllowRootUserAccess)) ?? false,
                AllowOtherUserAccess = (bool?)options.Get(nameof(AllowOtherUserAccess)) ?? false,
                PrintDebugInformation = (bool?)options.Get(nameof(PrintDebugInformation)) ?? false
            };
        }
    }
}
