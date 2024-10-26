using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public class FuseOptions : FileSystemOptions
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
        /// Gets whether the filesystem is read-only.
        /// </summary>
        public bool IsReadOnly { get; init; }

        /// <summary>
        /// Gets whether to print debugging information to the console.
        /// </summary>
        public bool PrintDebugInformation { get; init; }
    }
}