using System;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal abstract class HandleObject : IDisposable
    {
        public ICiphertextPath CiphertextPath { get; }

        protected HandleObject(ICiphertextPath ciphertextPath)
        {
            this.CiphertextPath = ciphertextPath;
        }

        public abstract void Dispose();
    }
}
