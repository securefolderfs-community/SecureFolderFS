using SecureFolderFS.Core.FileSystem.AppModels;

namespace SecureFolderFS.Core.FUSE.AppModels
{
    /// <inheritdoc cref="MountOptions"/>
    public class FuseMountOptions : MountOptions
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
    }
}