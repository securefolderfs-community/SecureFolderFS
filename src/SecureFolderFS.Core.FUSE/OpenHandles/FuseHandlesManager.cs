using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class FuseHandlesManager : BaseHandlesManager
    {
        public FuseHandlesManager(StreamsAccess streamsAccess)
            : base(streamsAccess)
        {
        }

        public IEnumerable<IDisposable> OpenHandles
        {
            get
            {
                lock (handles)
                    return handles.Values;
            }
        }

        /// <inheritdoc/>
        public override ulong OpenFileHandle(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            // Open ciphertext stream
            var ciphertextStream = new FileStream(ciphertextPath, new FileStreamOptions
            {
                // A file cannot be opened with both FileMode.Append and FileMode.Read, but opening it with
                // FileMode.Write would cause an error when writing, as the stream needs to be readable.
                Mode = mode == FileMode.Append ? FileMode.Open : mode,
                Access = FileAccess.ReadWrite,
                Share = share | FileShare.Delete,
                Options = options
            });

            // Open plaintext stream on top of ciphertext stream
            var PlaintextStream = streamsAccess.OpenPlaintextStream(ciphertextPath, ciphertextStream);
            if (PlaintextStream is null)
                return FileSystem.Constants.INVALID_HANDLE;

            // Flush ChunkAccess if the opened to Truncate
            if (mode == FileMode.Truncate)
                PlaintextStream.Flush();

            // Create handle
            var fileHandle = new FuseFileHandle(PlaintextStream, access, mode, Path.GetDirectoryName(ciphertextPath)!);
            var handle = handlesGenerator.ThreadSafeIncrement();

            lock (handles)
                handles.TryAdd(handle, fileHandle);

            return handle;
        }

        /// <inheritdoc/>
        public override ulong OpenDirectoryHandle(string ciphertextPath)
        {
            // Not supported, return invalid handle
            return FileSystem.Constants.INVALID_HANDLE;
        }

        /// <inheritdoc/>
        public override THandle? GetHandle<THandle>(ulong handleId)
            where THandle : class
        {
            lock (handles)
                return base.GetHandle<THandle>(handleId);
        }

        /// <inheritdoc/>
        public override void CloseHandle(ulong handle)
        {
            lock (handles)
                base.CloseHandle(handle);
        }
    }
}