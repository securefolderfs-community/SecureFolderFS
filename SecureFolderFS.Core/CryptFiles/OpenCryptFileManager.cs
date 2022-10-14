using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.CryptFiles;
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
        private readonly ChunkCachingStrategy _chunkCachingStrategy;
        private readonly IFileSystemStatsTracker? _statsTracker;

        public OpenCryptFileManager(Security security, ChunkCachingStrategy chunkCachingStrategy, IFileSystemStatsTracker? statsTracker)
        {
            _security = security;
            _chunkCachingStrategy = chunkCachingStrategy;
            _statsTracker = statsTracker;
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
            var chunkReader = new ChunkReader(_security, headerBuffer, streamsManager, _statsTracker);
            var chunkWriter = new ChunkWriter(_security, headerBuffer, streamsManager, _statsTracker);

            return _chunkCachingStrategy switch
            {
                ChunkCachingStrategy.RandomAccessMemoryCache => new DictionaryCacheChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _statsTracker),
                ChunkCachingStrategy.NoCache => new NonCachingChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _statsTracker),
                _ => throw new ArgumentOutOfRangeException(nameof(_chunkCachingStrategy))
            };
        }

        private void NotifyClosed(string ciphertextPath)
        {
            openCryptFiles.Remove(ciphertextPath);
        }
    }
}
