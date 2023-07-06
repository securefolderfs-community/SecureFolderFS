namespace SecureFolderFS.Core.FileSystem.Statistics
{
    public interface IFileSystemStatistics
    {
        #region IO

        void NotifyBytesRead(long amount);

        void NotifyBytesWritten(long amount);

        void NotifyBytesEncrypted(long amount);

        void NotifyBytesDecrypted(long amount);

        #endregion

        #region Chunks

        void NotifyChunkCacheMiss();

        void NotifyChunkCacheHit();

        void NotifyChunkAccess();

        #endregion

        #region DirectoryId

        void NotifyDirectoryIdCacheMiss();

        void NotifyDirectoryIdCacheHit();

        void NotifyDirectoryIdAccess();

        #endregion

        #region FileName

        void NotifyFileNameCacheMiss();

        void NotifyFileNameCacheHit();

        void NotifyFileNameAccess();

        #endregion
    }
}
