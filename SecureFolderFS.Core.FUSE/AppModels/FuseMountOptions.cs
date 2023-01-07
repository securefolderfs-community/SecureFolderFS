using SecureFolderFS.Core.FileSystem.AppModels;

namespace SecureFolderFS.Core.FUSE.AppModels
{
    /// <inheritdoc cref="MountOptions"/>
    public class FuseMountOptions : MountOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPath { get; init; } = null;
    }
}