using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    internal sealed class HandlesManager : IDisposable
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly HandleGenerator _handleGenerator;
        private readonly Dictionary<ulong, ObjectHandle> _openHandles;

        public HandlesManager(IStreamsAccess streamsAccess)
        {
            _streamsAccess = streamsAccess;
            _handleGenerator = new();
            _openHandles = new();
        }

        public IEnumerable<ObjectHandle> OpenHandles
        {
            get
            {
                lock (_openHandles)
                    return _openHandles.Values;
            }
        }

        public ulong? OpenHandleToFile(string ciphertextPath, FileMode mode, FileAccess access, FileShare share,
            FileOptions options)
        {
            var ciphertextStream = new FileStream(ciphertextPath, new FileStreamOptions
            {
                // A file cannot be opened with both FileMode.Append and FileMode.Read, but opening it with
                // FileMode.Write would cause an error when writing, as the stream needs to be readable.
                Mode = mode == FileMode.Append ? FileMode.Open : mode,
                Access = FileAccess.ReadWrite,
                Share = share,
                Options = options
            });
            var cleartextStream = _streamsAccess.OpenCleartextStream(ciphertextPath, ciphertextStream);

            if (cleartextStream is null)
                return null;

            if (mode == FileMode.Truncate)
                cleartextStream.Flush();

            var fileHandle = new FuseFileHandle(cleartextStream, access, mode, Path.GetDirectoryName(ciphertextPath)!);
            var handle = _handleGenerator.ThreadSafeIncrementAndGet();

            lock (_openHandles)
                _openHandles.TryAdd(handle, fileHandle);

            return handle;
        }

        public THandle? GetHandle<THandle>(ulong? handle)
            where THandle : ObjectHandle
        {
            if (handle == null)
                return null;

            lock (_openHandles)
            {
                _openHandles.TryGetValue(handle.Value, out var handleObject);
                return (THandle?)handleObject;
            }
        }

        public void CloseHandle(ulong? handle)
        {
            if (handle == null)
                return;

            lock (_openHandles)
            {
                _openHandles.Remove(handle.Value, out var handleObject);
                handleObject?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openHandles.Values.DisposeCollection();
            _openHandles.Clear();
        }

        private sealed class HandleGenerator
        {
            private ulong _handleCounter;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong ThreadSafeIncrementAndGet()
            {
                Interlocked.Increment(ref _handleCounter);
                return _handleCounter;
            }
        }
    }
}