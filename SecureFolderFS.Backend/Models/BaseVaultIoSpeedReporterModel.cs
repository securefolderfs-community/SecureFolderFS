using SecureFolderFS.Sdk.Tracking;

namespace SecureFolderFS.Backend.Models
{
    public abstract class BaseVaultIoSpeedReporterModel : IFileSystemStatsTracker
    {
        public virtual void AddBytesRead(long amount)
        {
        }

        public virtual void AddBytesWritten(long amount)
        {
        }

        public virtual void AddBytesEncrypted(long amount)
        {
        }

        public virtual void AddBytesDecrypted(long amount)
        {
        }

        public virtual void AddChunkCacheMiss()
        {
        }

        public virtual void AddChunkCacheHit()
        {
        }

        public virtual void AddChunkAccess()
        {
        }

        public virtual void AddDirectoryIdCacheMiss()
        {
        }

        public virtual void AddDirectoryIdCacheHit()
        {
        }

        public virtual void AddDirectoryIdAccess()
        {
        }

        public virtual void AddFileNameCacheMiss()
        {
        }

        public virtual void AddFileNameCacheHit()
        {
        }

        public virtual void AddFileNameAccess()
        {
        }

        public abstract void Dispose();
    }
}
