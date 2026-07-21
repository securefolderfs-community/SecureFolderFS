using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class MacFuseHandlesManager : BaseHandlesManager
    {
        public MacFuseHandlesManager(StreamsAccess streamsAccess, VirtualFileSystemOptions fileSystemOptions)
            : base(streamsAccess, fileSystemOptions)
        {
        }

        /// <summary>
        /// Provides an enumerable collection of all currently active handles managed by the instance.
        /// </summary>
        /// <remarks>
        /// The handles are returned as a snapshot of the internal collection to prevent race conditions
        /// when accessing or enumerating the data. Any modifications to the collection are thread-safe
        /// and do not affect the snapshot returned by this property.
        /// </remarks>
        public IEnumerable<IDisposable> OpenHandles
        {
            get
            {
                // Return a snapshot - the live collection could be mutated
                // by another thread while the caller is enumerating it
                lock (handles)
                    return handles.Values.ToArray();
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

                // Writes require read access as well (chunks are read-modify-write), but read-only
                // opens must not request write access. Otherwise, files with read-only permissions
                // (or vaults on read-only media) could not be opened at all
                Access = access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite,
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
            var fileHandle = new MacFuseFileHandle(plaintextStream, access, mode, Path.GetDirectoryName(ciphertextPath)!);
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
