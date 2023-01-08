using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    /// <summary>
    /// Represents a directory handle on the virtual file system.
    /// </summary>
    internal sealed class DirectoryHandle : ObjectHandle
    {
        /// <inheritdoc/>
        public override void Dispose()
        {
        }
    }
}