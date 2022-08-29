using System;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.BufferHolders;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    internal sealed class ChunkAccessFactory
    {
        private readonly VaultVersion _vaultVersion;
        private readonly ChunkCachingStrategy _chunkCachingStrategy;
        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;
        private readonly ISecurity _security;

        public ChunkAccessFactory(ISecurity security, VaultVersion vaultVersion, ChunkCachingStrategy chunkCachingStrategy, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            _security = security;
            _vaultVersion = vaultVersion;
            _chunkCachingStrategy = chunkCachingStrategy;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public IChunkReader GetChunkReader(CiphertextStreamsManager ciphertextStreamsManager, CleartextHeaderBuffer fileHeader)
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new ChunkReader(_security, fileHeader, ciphertextStreamsManager, _fileSystemStatsTracker);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IChunkWriter GetChunkWriter(CiphertextStreamsManager ciphertextStreamsManager, CleartextHeaderBuffer fileHeader)
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new ChunkWriter(_security, fileHeader, ciphertextStreamsManager, _fileSystemStatsTracker);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IChunkAccess GetChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter)
        {
            return _chunkCachingStrategy switch
            {
                ChunkCachingStrategy.NoCache => new NonCachingChunkAccess(_security.ContentCrypt, chunkReader, chunkWriter, _fileSystemStatsTracker),
                ChunkCachingStrategy.RandomAccessMemoryCache => new DictionaryChunkAccess(_security.ContentCrypt, chunkReader, chunkWriter, _fileSystemStatsTracker)!,
                _ => throw new ArgumentOutOfRangeException(nameof(_chunkCachingStrategy)),
            };
        }
    }
}
