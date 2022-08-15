using System;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Sdk.Tracking;

namespace SecureFolderFS.Core.FileNames.Receivers
{
    internal abstract class BaseFileNameReceiver : IFileNameReceiver
    {
        protected readonly ISecurity security;

        protected readonly IFileSystemStatsTracker fileSystemStatsTracker;

        private bool _disposed;

        protected BaseFileNameReceiver(ISecurity security, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            this.security = security;
            this.fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public virtual string GetCleartextFileName(DirectoryId directoryId, string ciphertextFileName)
        {
            AssertNotDisposed();

            fileSystemStatsTracker?.AddFileNameAccess();

            var cleartextFileName = security.ContentCryptor.FileNameCryptor.DecryptFileName(
                PathHelpers.RemoveExtension(ciphertextFileName, Constants.ENCRYPTED_FILE_EXTENSION), directoryId);

            SetCleartextFileName(directoryId, ciphertextFileName, cleartextFileName);
            SetCiphertextFileName(directoryId, cleartextFileName, ciphertextFileName);

            return cleartextFileName;
        }

        public virtual string GetCiphertextFileName(DirectoryId directoryId, string cleartextFileName)
        {
            AssertNotDisposed();

            fileSystemStatsTracker?.AddFileNameAccess();

            var ciphertextFileName = PathHelpers.AppendExtension(security.ContentCryptor.FileNameCryptor.EncryptFileName(cleartextFileName, directoryId),
                    Constants.ENCRYPTED_FILE_EXTENSION);

            SetCiphertextFileName(directoryId, cleartextFileName, ciphertextFileName);
            SetCleartextFileName(directoryId, ciphertextFileName, cleartextFileName);

            return ciphertextFileName;
        }

        public abstract void SetCleartextFileName(DirectoryId directoryId, string ciphertextFileName, string cleartextFileName);

        public abstract void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName);

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
        }

        protected internal sealed class FileNameWithDirectoryId
        {
            private readonly DirectoryId _directoryId;

            private readonly string _fileName;

            public FileNameWithDirectoryId(DirectoryId directoryId, string fileName)
            {
                _directoryId = directoryId;
                _fileName = fileName;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_directoryId, _fileName);
            }

            public override bool Equals(object obj)
            {
                if (obj is FileNameWithDirectoryId ciphertextFileNameWithDirectoryId)
                {
                    return _directoryId.Equals(ciphertextFileNameWithDirectoryId._directoryId)
                           && _fileName.Equals(ciphertextFileNameWithDirectoryId._fileName);
                }

                return base.Equals(obj);
            }
        }
    }
}
