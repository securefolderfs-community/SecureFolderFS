using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class FuseHandlesManager : BaseHandlesManager
    {
        public FuseHandlesManager(StreamsAccess streamsAccess, VirtualFileSystemOptions fileSystemOptions)
            : base(streamsAccess, fileSystemOptions)
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
            // Make sure the handles manager was not disposed
            if (disposed)
                return FileSystem.Constants.INVALID_HANDLE;

            if (fileSystemOptions.IsReadOnly && mode.IsWriteFlag())
                return FileSystem.Constants.INVALID_HANDLE;

            // Open ciphertext stream
            var ciphertextStream = new FileStream(ciphertextPath, new FileStreamOptions()
            {
                // A file cannot be opened with both FileMode.Append and FileMode.Read, but opening it with
                // FileMode.Write would cause an error when writing, as the stream needs to be readable.
                Mode = mode == FileMode.Append ? FileMode.Open : mode,
                Access = FileAccess.ReadWrite,
                Share = share | FileShare.Delete,
                Options = options
            });

            // Open plaintext stream on top of ciphertext stream
            var plaintextStream = streamsAccess.TryOpenPlaintextStream(ciphertextPath, ciphertextStream);
            if (plaintextStream is null)
                return FileSystem.Constants.INVALID_HANDLE;

            // Flush ChunkAccess if the opened to Truncate
            if (mode == FileMode.Truncate)
                plaintextStream.Flush();

            // Create handle
            var fileHandle = new FuseFileHandle(plaintextStream, access, mode, Path.GetDirectoryName(ciphertextPath)!);
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