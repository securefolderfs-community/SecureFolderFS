using System;

namespace SecureFolderFS.Core.FileSystem.Analytics
{
    /// <summary>
    /// Provides module for reporting live vault statistics.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    [Obsolete]
    public interface IFileSystemStatsTracker : IDisposable
    {
        #region IO

        void AddBytesRead(long amount);

        void AddBytesWritten(long amount);

        void AddBytesEncrypted(long amount);

        void AddBytesDecrypted(long amount);

        #endregion

        #region Chunks

        void AddChunkCacheMiss();

        void AddChunkCacheHit();

        void AddChunkAccess();

        #endregion

        #region DirectoryId

        void AddDirectoryIdCacheMiss();

        void AddDirectoryIdCacheHit();

        void AddDirectoryIdAccess();

        #endregion

        #region FileName

        void AddFileNameCacheMiss();

        void AddFileNameCacheHit();

        void AddFileNameAccess();

        #endregion
    }
}
