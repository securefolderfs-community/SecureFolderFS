using System;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileNames.Receivers;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Tracking;

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
            this._vaultVersion = vaultVersion;
            this._security = security;
            this._fileSystemStatsTracker = fileSystemStatsTracker;
            this._fileNameCachingStrategy = fileNameCachingStrategy;
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
