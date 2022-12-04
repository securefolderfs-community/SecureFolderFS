using SecureFolderFS.Core.FileSystem.Models;

namespace SecureFolderFS.Core.Dokany.Models
{
    /// <inheritdoc cref="MountOptions"/>
    public sealed class DokanyMountOptions : MountOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted.
        /// </summary>
        public required string MountPath { get; init; }
    }
}
