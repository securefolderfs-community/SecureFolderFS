using System.IO;
using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Streams.InternalStreams;
using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.Streams
{
    internal sealed class CiphertextFileStream : FileStream, ICiphertextFileStream, ICiphertextFileStreamInternal, IBaseFileStreamInternal
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

        SafeFileHandle IBaseFileStreamInternal.DangerousGetInternalSafeFileHandle()
        {
            return SafeFileHandle;
        }
    }
}
