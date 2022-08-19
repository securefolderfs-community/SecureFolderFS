using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class HandlesCollection : IDisposable
    {
        private readonly IFileStreamReceiver _fileStreamReceiver;
        private readonly ConcurrentDictionary<long, HandleObject> _openHandles;
        private readonly HandleGenerator _handleGenerator;

        private bool _disposed;

        public HandlesCollection(IFileStreamReceiver fileStreamReceiver)
        {
            _fileStreamReceiver = fileStreamReceiver;
            _openHandles = new();
            _handleGenerator = new HandleGenerator();
        }

        public long OpenHandleToDirectory(ICiphertextPath ciphertextPath, FileMode mode, DokanNet.FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            var directoryHandle = DirectoryHandle.Open(ciphertextPath, mode, access, share, options);
            var handle = Constants.FileSystem.INVALID_HANDLE;

            if (directoryHandle is not null)
            {
                handle = _handleGenerator.ThreadSafeIncrementAndGet();
                _openHandles.TryAdd(handle, directoryHandle);
            }

            return handle;
        }

        public long OpenHandleToFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            var fileHandle = FileHandle.Open(ciphertextPath, _fileStreamReceiver, mode, access, share, options);
            var handle = Constants.FileSystem.INVALID_HANDLE;

            if (fileHandle is not null)
            {
                handle = _handleGenerator.ThreadSafeIncrementAndGet();
                _openHandles.TryAdd(handle, fileHandle);
            }

            return handle;
        }

        public HandleObject GetHandle(long fileHandle)
        {
            AssertNotDisposed();

            if (_openHandles.TryGetValue(fileHandle, out var handle))
            {
                return handle;
            }

            return null;
        }

        public void Close(long handle)
        {
            AssertNotDisposed();

            if (handle != Constants.FileSystem.INVALID_HANDLE && _openHandles.ContainsKey(handle))
            {
                _openHandles.TryRemove(handle, out var hUnknownHandle);
                hUnknownHandle?.Dispose();
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _openHandles.Values.DisposeCollection();
                _openHandles.Clear();
            }
        }

        private class HandleGenerator
        {
            private long _handleCounter;

            public long ThreadSafeIncrementAndGet()
            {
                Interlocked.Increment(ref _handleCounter);
                return _handleCounter;
            }
        }
    }
}
