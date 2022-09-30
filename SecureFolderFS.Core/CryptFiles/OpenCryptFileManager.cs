using System;
using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.CryptFiles
{
    /// <inheritdoc cref="ICryptFileManager"/>
    internal sealed class OpenCryptFileManager : BaseCryptFileManager
    {
        private readonly Security _security;

        public OpenCryptFileManager(Security security)
        {
            _security = security;
        }

        /// <inheritdoc/>
        protected override ICryptFile? GetCryptFile(string ciphertextPath, BufferHolder headerBuffer)
        {
            if (headerBuffer is not HeaderBuffer headerBuffer2)
                return null;

            var streamsManager = new StreamsManager();
            var chunkAccess = GetChunkAccess(streamsManager, headerBuffer2);

            return new OpenCryptFile(ciphertextPath, _security, headerBuffer2, chunkAccess, streamsManager, NotifyClosed);
        }

        private IChunkAccess GetChunkAccess(IStreamsManager streamsManager, HeaderBuffer headerBuffer)
        {
            throw new NotImplementedException(); // TODO: Implement this method
        }

        private void NotifyClosed(string ciphertextPath)
        {
            openCryptFiles.Remove(ciphertextPath);
        }
    }
}
