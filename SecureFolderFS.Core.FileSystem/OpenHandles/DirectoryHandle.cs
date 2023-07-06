using System;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    /// <summary>
    /// Represents a directory handle on the virtual file system.
    /// </summary>
    public abstract class DirectoryHandle : IDisposable
    {
        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
