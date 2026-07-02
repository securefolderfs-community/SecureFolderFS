using System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class DokanyHandlesManager : BaseHandlesManager
    {
        private readonly Security _security;

        public DokanyHandlesManager(Security security, StreamsAccess streamsAccess, VirtualFileSystemOptions fileSystemOptions)
            : base(streamsAccess, fileSystemOptions)
        {
            _security = security;
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
            var ciphertextStream = new FileStream(ciphertextPath, mode, access, share, 4096, options);

            // Open plaintext stream on top of ciphertext stream
            var plaintextStream = streamsAccess.TryOpenPlaintextStream(ciphertextPath, ciphertextStream);
            if (plaintextStream is null)
                return FileSystem.Constants.INVALID_HANDLE;

            // Flush ChunkAccess if opened with Truncate flag
            if (mode == FileMode.Truncate)
                plaintextStream.Flush();

            // Create handle
            var fileHandle = new DokanyFileHandle(plaintextStream, _security);
            var handleId = handlesGenerator.ThreadSafeIncrement();

            // Lock collection and add handle
            lock (handlesLock)
                handles.TryAdd(handleId, fileHandle);

            return handleId;
        }

        /// <inheritdoc/>
        public override ulong OpenDirectoryHandle(string ciphertextPath)
        {
            // Not supported, return invalid handle
            return FileSystem.Constants.INVALID_HANDLE;
        }
    }
}
