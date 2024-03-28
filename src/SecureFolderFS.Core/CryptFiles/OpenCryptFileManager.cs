using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Shared.Helpers;
using System;

namespace SecureFolderFS.Core.CryptFiles
{
    /// <inheritdoc cref="ICryptFileManager"/>
    internal sealed class OpenCryptFileManager : BaseCryptFileManager
    {
        private readonly Security _security;
        private readonly bool _enableChunkCache;
        private readonly IFileSystemStatistics _fileSystemStatistics;

        public OpenCryptFileManager(Security security, bool enableChunkCache, IFileSystemStatistics fileSystemStatistics)
        {
            _security = security;
            _enableChunkCache = enableChunkCache;
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        protected override ICryptFile GetCryptFile(string id, BufferHolder headerBuffer)
        {
            if (headerBuffer is not HeaderBuffer headerBuffer2)
                throw new ArgumentException($"{nameof(headerBuffer)} does not implement {nameof(HeaderBuffer)}.");

            var streamsManager = new StreamsManager();
            var chunkAccess = GetChunkAccess(streamsManager, headerBuffer2);

            return new OpenCryptFile(id, _security, headerBuffer2, chunkAccess, streamsManager, NotifyClosed);
        }

        private IChunkAccess GetChunkAccess(IStreamsManager streamsManager, HeaderBuffer headerBuffer)
        {
            var chunkReader = new ChunkReader(_security, headerBuffer, streamsManager, _fileSystemStatistics);
            var chunkWriter = new ChunkWriter(_security, headerBuffer, streamsManager, _fileSystemStatistics);

            return _enableChunkCache
                ? new CachingChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics)
                : new InstantChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics);
        }

        private void NotifyClosed(string ciphertextPath)
        {
            lock (openCryptFiles)
            {
                openCryptFiles.Remove(ciphertextPath);
            }
        }
    }
}
