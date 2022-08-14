using System;
using System.IO;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.UnsafeNative;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Core.Streams.Receiver;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class FileHandle : HandleObject
    {
        private bool _disposed;

        public ICleartextFileStream CleartextFileStream { get; }

        private FileHandle(ICleartextFileStream cleartextFileStream)
        {
            CleartextFileStream = cleartextFileStream;
        }

        public bool SetFileTime(ref long ct, ref long lat, ref long lwt)
        {
            AssertNotDisposed();

            var hFile = CleartextFileStream.AsBaseFileStreamInternal().DangerousGetInternalSafeFileHandle();
            return UnsafeNativeApis.SetFileTime(hFile, ref ct, ref lat, ref lwt);
        }

        public static FileHandle Open(ICiphertextPath ciphertextPath, IFileStreamReceiver fileStreamReceiver, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            var cleartextFileStream = fileStreamReceiver.OpenFileStreamToCleartextFile(ciphertextPath, mode, access, share, options);
            return new FileHandle(cleartextFileStream);
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        public override void Dispose()
        {
            _disposed = true;
            CleartextFileStream?.Dispose();
        }
    }
}
