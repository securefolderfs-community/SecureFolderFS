using System;
using SecureFolderFS.Core.Chunks.Receivers;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal sealed class ChunkReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IChunkFactory _chunkFactory;

        private readonly ChunkCachingStrategy _chunkCachingStrategy;

        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;

        public ChunkReceiverFactory(VaultVersion vaultVersion, IChunkFactory chunkFactory, ChunkCachingStrategy chunkCachingStrategy, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            _vaultVersion = vaultVersion;
            _chunkFactory = chunkFactory;
            _chunkCachingStrategy = chunkCachingStrategy;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public IChunkReaderDeprecated GetChunkReader(ISecurity security, CiphertextStreamsManager ciphertextStreamsManager, IFileHeader fileHeader)
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new ChunkReader(security, fileHeader, _chunkFactory, ciphertextStreamsManager, _fileSystemStatsTracker);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IChunkWriterDeprecated GetChunkWriter(ISecurity security, CiphertextStreamsManager ciphertextStreamsManager, IFileHeader fileHeader)
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new ChunkWriter(security, fileHeader, ciphertextStreamsManager, _fileSystemStatsTracker);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IChunkReceiver GetChunkReceiver(IChunkReaderDeprecated chunkReader, IChunkWriterDeprecated chunkWriter)
        {
            return _chunkCachingStrategy switch
            {
                ChunkCachingStrategy.NoCache => new InstantAccessBasedChunkReceiver(chunkReader, chunkWriter, _fileSystemStatsTracker),
                ChunkCachingStrategy.RandomAccessMemoryCache => new RandomAccessMemoryBasedChunkReceiver(chunkReader, chunkWriter, _fileSystemStatsTracker),
                ChunkCachingStrategy.MemoryCache => new MemoryCacheBasedChunkReceiver(chunkReader, chunkWriter, _fileSystemStatsTracker),
                _ => throw new ArgumentOutOfRangeException(nameof(_chunkCachingStrategy)),
            };
        }
    }
}
