using System;
using System.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Sdk.Streams;
using SecureFolderFS.Core.UnsafeNative;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class FileHandle : HandleObject
    {
        private bool _disposed;

        public IVaultFile VaultFile { get; }

        public ICleartextFileStream CleartextFileStream { get; }

        private FileHandle(IVaultFile vaultFile, ICleartextFileStream cleartextFileStream)
            : base(vaultFile.CiphertextPath)
        {
            this.VaultFile = vaultFile;
            this.CleartextFileStream = cleartextFileStream;
        }

        public bool SetFileTime(ref long ct, ref long lat, ref long lwt)
        {
            AssertNotDisposed();

            var hFile = CleartextFileStream.AsBaseFileStreamInternal().GetSafeFileHandle();
            return UnsafeNativeApis.SetFileTime(hFile, ref ct, ref lat, ref lwt);
        }

        public static FileHandle Open(ICiphertextPath ciphertextPath, IVaultStorageReceiver vaultStorageReceiver, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            var vaultFile = vaultStorageReceiver.OpenVaultFile(ciphertextPath);
            var cleartextFileStream = vaultFile.OpenStream(mode, access, share, options);

            return new FileHandle(vaultFile, cleartextFileStream);
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public override void Dispose()
        {
            _disposed = true;
            CleartextFileStream?.Dispose();
        }
    }
}
