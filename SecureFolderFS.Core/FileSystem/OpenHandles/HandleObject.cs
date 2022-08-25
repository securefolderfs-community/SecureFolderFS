using System;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal abstract class HandleObject : IDisposable
    {
        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
