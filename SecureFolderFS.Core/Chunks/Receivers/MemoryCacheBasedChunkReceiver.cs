using System;
using System.Runtime.Caching;
using System.Collections.Specialized;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.Tracking;

namespace SecureFolderFS.Core.Chunks.Receivers
{
    internal sealed class MemoryCacheBasedChunkReceiver : BaseChunkReceiver
    {
        private readonly MemoryCache _memoryCache;

        public MemoryCacheBasedChunkReceiver(IChunkReader chunkReader, IChunkWriter chunkWriter, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(chunkReader, chunkWriter, fileSystemStatsTracker)
        {
            this._memoryCache = new MemoryCache("SecureFolderFS_chunkcache", new NameValueCollection(Constants.IO.MAX_CACHED_CHUNKS));
        }

        public override ICleartextChunk GetChunk(long chunkNumber)
        {
            AssertNotDisposed();

            if (_memoryCache.Get(chunkNumber.ToString()) is not ICleartextChunk cleartextChunk)
            {
                fileSystemStatsTracker?.AddChunkCacheMiss();
                cleartextChunk = base.GetChunk(chunkNumber);
                SetChunk(chunkNumber, cleartextChunk);
            }
            else
            {
                fileSystemStatsTracker?.AddChunkAccess();
                fileSystemStatsTracker?.AddChunkCacheHit();
            }

            return cleartextChunk;
        }

        public override void SetChunk(long chunkNumber, ICleartextChunk cleartextChunk)
        {
            AssertNotDisposed();

            _memoryCache.Set(chunkNumber.ToString(), cleartextChunk, new CacheItemPolicy()
            {
                AbsoluteExpiration = new DateTimeOffset().ToOffset(TimeSpan.FromMinutes(1d)),
                RemovedCallback = (e) => base.SetChunk(Convert.ToInt64(e.CacheItem.Key), (ICleartextChunk)e.CacheItem.Value)
            });
        }

        public override void Flush()
        {
            AssertNotDisposed();

            _memoryCache.Dispose();
        }
    }
}
