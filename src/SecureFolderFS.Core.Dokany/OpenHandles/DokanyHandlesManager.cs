using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.IO;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class DokanyHandlesManager : BaseHandlesManager
    {
        public DokanyHandlesManager(StreamsAccess streamsAccess, FileSystemOptions fileSystemOptions)
            : base(streamsAccess, fileSystemOptions)
        {
        }

        /// <inheritdoc/>
        public override ulong OpenFileHandle(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            // Make sure the handles manager was not disposed
            if (disposed)
                return FileSystem.Constants.INVALID_HANDLE;

            if (fileSystemOptions.IsReadOnly && mode.IsWriteFlag())
                return FileSystem.Constants.INVALID_HANDLE;

            // TODO: Temporary fix for file share issue
            //share = FileShare.ReadWrite | FileShare.Delete;

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
            var fileHandle = new Win32FileHandle(plaintextStream); // TODO: For now it's Win32FileHandle
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
