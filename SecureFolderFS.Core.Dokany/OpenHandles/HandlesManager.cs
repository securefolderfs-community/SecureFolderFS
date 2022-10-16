using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    internal sealed class HandlesManager : IDisposable
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly HandleGenerator _handleGenerator;
        private readonly Dictionary<long, ObjectHandle> _openHandles;

        public HandlesManager(IStreamsAccess streamsAccess)
        {
            _streamsAccess = streamsAccess;
            _handleGenerator = new();
            _openHandles = new();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long OpenHandleToFile(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            // Open stream
            var share2 = FileShare.ReadWrite | FileShare.Delete; // TODO: Use share2 because share is broken
            var ciphertextStream = new FileStream(ciphertextPath, mode, access, share2, 4096, options);
            var cleartextStream = _streamsAccess.OpenCleartextStream(ciphertextPath, ciphertextStream);

            if (cleartextStream is null)
                return Constants.FileSystem.INVALID_HANDLE;

            // Flush ChunkAccess if the opened to Truncate
            if (mode == FileMode.Truncate)
                cleartextStream.Flush();

            // Create handle
            var fileHandle = new Win32FileHandle(cleartextStream); // TODO: For now it's Win32FileHandle
            var handle = _handleGenerator.ThreadSafeIncrementAndGet();

            // Add handle and return
            _openHandles.TryAdd(handle, fileHandle);
            return handle;
        }

        public THandle? GetHandle<THandle>(long handle)
            where THandle : ObjectHandle
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

        private sealed class HandleGenerator
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
