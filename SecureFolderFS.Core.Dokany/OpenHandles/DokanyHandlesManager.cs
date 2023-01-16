using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
using System.IO;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="BaseHandlesManager"/>
    internal sealed class DokanyHandlesManager : BaseHandlesManager
    {
        public DokanyHandlesManager(IStreamsAccess streamsAccess)
            : base(streamsAccess)
        {
        }

        /// <inheritdoc/>
        public override ulong OpenFileHandle(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            share = FileShare.ReadWrite | FileShare.Delete; // TODO: Temporary fix for file share issue

            // Open ciphertext stream
            var ciphertextStream = new FileStream(ciphertextPath, mode, access, share, 4096, options);

            // Open cleartext stream on top of ciphertext stream
            var cleartextStream = streamsAccess.OpenCleartextStream(ciphertextPath, ciphertextStream);

            if (cleartextStream is null)
                return FileSystem.Constants.INVALID_HANDLE;

            // Flush ChunkAccess if the opened to Truncate
            if (mode == FileMode.Truncate)
                cleartextStream.Flush();

            // Create handle
            var fileHandle = new Win32FileHandle(cleartextStream); // TODO: For now it's Win32FileHandle
            var handleId = handlesGenerator.ThreadSafeIncrement();

            // Add handle
            handles.Add(handleId, fileHandle);

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
