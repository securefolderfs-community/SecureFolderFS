using System;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal abstract class BaseDirectoryIdReceiver : IDirectoryIdReceiver, IDisposable
    {
        protected readonly IDirectoryIdReader directoryIdReader;

        protected readonly IFileSystemStatsTracker fileSystemStatsTracker;

        private bool _disposed;

        protected BaseDirectoryIdReceiver(IDirectoryIdReader directoryIdReader, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            this.directoryIdReader = directoryIdReader;
            this.fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public virtual DirectoryId GetDirectoryId(string ciphertextPath)
        {
            AssertNotDisposed();

            fileSystemStatsTracker?.AddDirectoryIdAccess();

            return directoryIdReader.ReadDirectoryId(ciphertextPath);
        }

        public DirectoryId CreateNewDirectoryId()
        {
            AssertNotDisposed();

            fileSystemStatsTracker?.AddDirectoryIdAccess();

            return DirectoryId.CreateNew();
        }

        public abstract void RemoveDirectoryId(string ciphertextPath);

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
            directoryIdReader?.Dispose();
        }
    }
}
