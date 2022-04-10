using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class HandlesCollection : IDisposable
    {
        private readonly Dictionary<long, HandleObject> _openHandles;

        private readonly IVaultStorageReceiver _vaultStorageReceiver;

        private readonly HandleGenerator _handleGenerator;

        private bool _disposed;

        public HandlesCollection(IVaultStorageReceiver vaultStorageReceiver)
        {
            this._vaultStorageReceiver = vaultStorageReceiver;

            this._openHandles = new Dictionary<long, HandleObject>();
            this._handleGenerator = new HandleGenerator();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long OpenHandleToDirectory(ICiphertextPath ciphertextPath, FileMode mode, DokanNet.FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            var directoryHandle = DirectoryHandle.Open(ciphertextPath, _vaultStorageReceiver, mode, access, share, options);
            var handle = Constants.FileSystem.INVALID_HANDLE;

            if (directoryHandle != null)
            {
                handle = _handleGenerator.ThreadSafeIncrementAndGet();
                _openHandles.Add(handle, directoryHandle);
            }

            return handle;
        }

        public long OpenHandleToFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            var fileHandle = FileHandle.Open(ciphertextPath, _vaultStorageReceiver, mode, access, share, options);
            var handle = Constants.FileSystem.INVALID_HANDLE;

            if (fileHandle != null)
            {
                handle = _handleGenerator.ThreadSafeIncrementAndGet();
                _openHandles.Add(handle, fileHandle);
            }

            return handle;
        }

        public HandleObject GetHandle(long fileHandle)
        {
            AssertNotDisposed();

            if (_openHandles.TryGetValue(fileHandle, out HandleObject handle))
            {
                return handle;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Close(long handle)
        {
            AssertNotDisposed();

            if (handle != Constants.FileSystem.INVALID_HANDLE && _openHandles.ContainsKey(handle))
            {
                _openHandles[handle]?.Dispose();
                _openHandles.Remove(handle);
                return true;
            }

            return false;
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
