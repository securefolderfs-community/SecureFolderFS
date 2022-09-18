using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Shared.Helpers;
using System;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.CryptFiles
{
    /// <inheritdoc cref="IOpenCryptFileManager"/>
    internal sealed class OpenCryptFileManager : BaseOpenCryptFileManager
    {
        /// <inheritdoc/>
        protected override IOpenCryptFile GetOpenCryptFile(BufferHolder headerBuffer)
        {
            var streamsManager = new StreamsManager();
            var chunkAccess = GetChunkAccess(streamsManager, headerBuffer);

            var openCryptFile = new OpenCryptFile()
        }

        private IChunkAccess GetChunkAccess(StreamsManager streamsManager, BufferHolder headerBuffer)
        {

        }
    }
}
