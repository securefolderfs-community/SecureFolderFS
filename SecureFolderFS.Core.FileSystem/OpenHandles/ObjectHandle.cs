using System;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    /// <summary>
    /// Represents a generic virtual file system handle.
    /// </summary>
    public abstract class ObjectHandle : IDisposable
    {
        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
