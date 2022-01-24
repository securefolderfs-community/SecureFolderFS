using SecureFolderFS.Core.Tracking;

namespace SecureFolderFS.CLI
{
    internal sealed class ConsoleLoggingFileSystemStatsTracker : IFileSystemStatsTracker
    {
        private long _chunkAccess;

        private long _chunkMiss;

        private long _chunkHit;

        private readonly string _formatString = "Cache Access: {0} | Cache Miss: {1} | Cache Hit {2}";

        public void AddBytesDecrypted(long amount)
        {
        }

        public void AddBytesEncrypted(long amount)
        {
        }

        public void AddBytesRead(long amount)
        {
        }

        public void AddBytesWritten(long amount)
        {
        }

        public void AddChunkAccess()
        {
            _chunkAccess++;
            WriteFormatted();
        }

        public void AddChunkCacheHit()
        {
            _chunkHit++;
            WriteFormatted();
        }

        public void AddChunkCacheMiss()
        {
            _chunkMiss++;
            WriteFormatted();
        }

        public void AddDirectoryIdAccess()
        {
        }

        public void AddDirectoryIdCacheHit()
        {
        }

        public void AddDirectoryIdCacheMiss()
        {
        }

        private void WriteFormatted()
        {
            //Console.WriteLine(string.Format(_formatString, _chunkAccess, _chunkMiss, _chunkHit));
        }

        public void Dispose()
        {
        }

        public void AddFileNameCacheMiss()
        {
        }

        public void AddFileNameCacheHit()
        {
        }

        public void AddFileNameAccess()
        {
        }
    }
}
