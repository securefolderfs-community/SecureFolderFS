using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Sdk.Streams;
using System.IO;

namespace SecureFolderFS.Core.Streams
{
    internal sealed class CiphertextFileStream : FileStream, ICiphertextFileStream
    {
        public bool IsDisposed { get; private set; }

        public CiphertextFileStream(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share)
            : base(ciphertextPath.Path, mode, access, share)
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = disposing;

            base.Dispose(disposing);
        }
    }
}
