using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class HandlesManager : IDisposable
    {
        private readonly IFileStreamReceiver _fileStreamReceiver;
        private readonly Dictionary<long, HandleObject> _openHandles;
        private readonly HandleGenerator _handleGenerator;

        public HandlesManager(IFileStreamReceiver fileStreamReceiver)
        {
            _fileStreamReceiver = fileStreamReceiver;
            _openHandles = new();
            _handleGenerator = new HandleGenerator();
        }

        public long OpenHandleToFile(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            // Open stream
            var share2 = FileShare.ReadWrite | FileShare.Delete; // TODO: Use share2 because share is broken
            var ciphertextStream = new FileStream(ciphertextPath, mode, access, share2, 4096, options);
            var cleartextStream = _fileStreamReceiver.OpenCleartextStream(ciphertextPath, ciphertextStream);

            // Flush ChunkAccess if the opened to Truncate
            if (mode == FileMode.Truncate)
                cleartextStream.Flush();

            // Create handle
            var fileHandle = new FileHandle(cleartextStream);
            var handle = _handleGenerator.ThreadSafeIncrementAndGet();

            // Add handle and return
            _openHandles.TryAdd(handle, fileHandle);
            return handle;
        }

        public THandle? GetHandle<THandle>(long handle)
            where THandle : HandleObject
        {
            if (handle == Constants.FileSystem.INVALID_HANDLE)
                return null;

            _openHandles.TryGetValue(handle, out var handleObject);
            return (THandle?)handleObject;
        }

        public void CloseHandle(long handle)
        {
            if (handle == Constants.FileSystem.INVALID_HANDLE)
                return;

            _openHandles.Remove(handle, out var handleObject);
            handleObject?.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openHandles.Values.DisposeCollection();
            _openHandles.Clear();
        }

        private class HandleGenerator
        {
            private long _handleCounter;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long ThreadSafeIncrementAndGet()
            {
                Interlocked.Increment(ref _handleCounter);
                return _handleCounter;
            }
        }
    }
}
