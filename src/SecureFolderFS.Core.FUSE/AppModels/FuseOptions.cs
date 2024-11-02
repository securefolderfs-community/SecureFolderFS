using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public sealed class FuseOptions : FileSystemOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPoint { get; init; } = null;

        /// <summary>
        /// Gets whether the root user should have access to the filesystem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requires <i>user_allow_other</i> to be uncommented in /etc/fuse.conf.
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
        /// Requires <i>user_allow_other</i> to be uncommented in /etc/fuse.conf.
        /// </para>
        /// <para>
        /// This option is mutually exclusive with <see cref="AllowRootUserAccess"/>.
        /// </para>
        /// </remarks>
        public bool AllowOtherUserAccess { get; init; }

        /// <summary>
        /// Gets whether to print debugging information to the console.
        /// </summary>
        public bool PrintDebugInformation { get; init; } // TODO: Use ILogger in the base class

        public static FuseOptions ToOptions(IDictionary<string, object> options, IFolder contentFolder)
        {
            return new()
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? new HealthStatistics(contentFolder),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? new FileSystemStatistics(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? true,

                // FUSE specific
                MountPoint = (string?)options.Get(nameof(MountPoint)),
                AllowRootUserAccess = (bool?)options.Get(nameof(AllowRootUserAccess)) ?? false,
                AllowOtherUserAccess = (bool?)options.Get(nameof(AllowOtherUserAccess)) ?? false,
                PrintDebugInformation = (bool?)options.Get(nameof(AllowOtherUserAccess)) ?? false
            };
        }
    }
}