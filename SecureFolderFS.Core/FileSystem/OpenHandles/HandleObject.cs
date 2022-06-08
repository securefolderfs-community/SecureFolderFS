using System;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal abstract class HandleObject : IDisposable
    {
        public ICiphertextPath CiphertextPath { get; }

        protected HandleObject(ICiphertextPath ciphertextPath)
        {
            CiphertextPath = ciphertextPath;
        }

        public abstract void Dispose();
    }
}
