using System;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <summary>
    /// Represents a generic virtual file system handle.
    /// </summary>
    internal abstract class ObjectHandle : IDisposable
    {
        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
