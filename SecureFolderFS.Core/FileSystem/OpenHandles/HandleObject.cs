using System;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal abstract class HandleObject : IDisposable
    {
        public abstract void Dispose();
    }
}
