using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileNames.Receivers;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.VaultDataStore;
using System;

namespace SecureFolderFS.Core.FileNames.Factory
{
    internal sealed class FileNameReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly ISecurity _security;

        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;

        private readonly FileNameCachingStrategy _fileNameCachingStrategy;

        public FileNameReceiverFactory(VaultVersion vaultVersion, ISecurity security, IFileSystemStatsTracker fileSystemStatsTracker, FileNameCachingStrategy fileNameCachingStrategy)
        {
            _vaultVersion = vaultVersion;
            _security = security;
            _fileSystemStatsTracker = fileSystemStatsTracker;
            _fileNameCachingStrategy = fileNameCachingStrategy;
        }

        public IFileNameReceiver GetFileNameReceiver()
        {
            return _fileNameCachingStrategy switch
            {
                FileNameCachingStrategy.RandomAccessMemoryCache => new RandomAccessMemoryBasedFileNameReceiver(_security, _fileSystemStatsTracker),
                FileNameCachingStrategy.NoCache => new InstantAccessBasedFileNameReceiver(_security, _fileSystemStatsTracker),
                _ => throw new ArgumentOutOfRangeException(nameof(FileNameCachingStrategy)),
            };
        }
    }
}
