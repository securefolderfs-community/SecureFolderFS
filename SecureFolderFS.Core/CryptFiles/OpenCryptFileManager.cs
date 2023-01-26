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
        private readonly IFileSystemStatistics? _fileSystemStatistics;

        public OpenCryptFileManager(Security security, ChunkCachingStrategy chunkCachingStrategy, IFileSystemStatistics? fileSystemStatistics)
        {
            _security = security;
            _chunkCachingStrategy = chunkCachingStrategy;
            _fileSystemStatistics = fileSystemStatistics;
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
            var chunkReader = new ChunkReader(_security, headerBuffer, streamsManager, _fileSystemStatistics);
            var chunkWriter = new ChunkWriter(_security, headerBuffer, streamsManager, _fileSystemStatistics);

            return _chunkCachingStrategy switch
            {
                ChunkCachingStrategy.RandomAccessMemoryCache => new CachingChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics),
                ChunkCachingStrategy.NoCache => new InstantChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics),
                _ => throw new ArgumentOutOfRangeException(nameof(_chunkCachingStrategy))
            };
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
