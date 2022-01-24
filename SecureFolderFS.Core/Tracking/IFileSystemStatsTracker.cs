using System;

namespace SecureFolderFS.Core.Tracking
{
    /// <summary>
    /// Provides module for reporting live vault statistics.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IFileSystemStatsTracker : IDisposable
    {
        #region IO

        void AddBytesRead(long amount);

        void AddBytesWritten(long amount);

        void AddBytesEncrypted(long amount);

        void AddBytesDecrypted(long amount);

        #endregion

        #region Chunks

        void AddChunkAccess();

        void AddChunkCacheMiss();

        void AddChunkCacheHit();

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
