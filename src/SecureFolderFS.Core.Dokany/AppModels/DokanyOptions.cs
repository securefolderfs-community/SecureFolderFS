using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.Dokany.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public sealed class DokanyOptions : FileSystemOptions
    {
        /// <summary>
        /// Gets the path where the file system should be mounted. If a null value is given, default mount point will be used.
        /// </summary>
        public string? MountPath { get; init; }
    }
}
