using System;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal sealed class DirectoryIdReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly DirectoryIdCachingStrategy _directoryIdCachingStrategy;

        private readonly IFileOperations _fileOperations;

        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;

        public DirectoryIdReceiverFactory(VaultVersion vaultVersion, DirectoryIdCachingStrategy directoryIdCachingStrategy, IFileOperations fileOperations, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            this._vaultVersion = vaultVersion;
            this._directoryIdCachingStrategy = directoryIdCachingStrategy;
            this._fileOperations = fileOperations;
            this._fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public IDirectoryIdReader GetDirectoryIdReader()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new DirectoryIdReader(_fileOperations);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }

        public IDirectoryIdReceiver GetDirectoryIdReceiver(IDirectoryIdReader directoryIdReader)
        {
            return _directoryIdCachingStrategy switch
            {
                DirectoryIdCachingStrategy.NoCache => new InstantAccessBasedDirectoryIdReceiver(directoryIdReader, _fileSystemStatsTracker),
                DirectoryIdCachingStrategy.RandomAccessMemoryCache => new RandomAccessMemoryBasedDirectoryIdReceiver(directoryIdReader, _fileSystemStatsTracker),
                _ => throw new ArgumentOutOfRangeException(nameof(ChunkCachingStrategy)),
            };
        }
    }
}
