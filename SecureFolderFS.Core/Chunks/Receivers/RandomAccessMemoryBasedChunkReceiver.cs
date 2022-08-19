using System.Collections.Concurrent;
using System.Linq;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Chunks.Receivers
{
    internal sealed class RandomAccessMemoryBasedChunkReceiver : BaseChunkReceiver
    {
        private readonly ConcurrentDictionary<long, ICleartextChunk> _chunks;

        public RandomAccessMemoryBasedChunkReceiver(IChunkReader chunkReader, IChunkWriter chunkWriter, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(chunkReader, chunkWriter, fileSystemStatsTracker)
        {
            _chunks = new(3, Constants.IO.MAX_CACHED_CHUNKS);
        }

        public override ICleartextChunk GetChunk(long chunkNumber)
        {
            AssertNotDisposed();

            if (!_chunks.TryGetValue(chunkNumber, out ICleartextChunk cleartextChunk))
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

            if (_chunks.Count >= Constants.IO.MAX_CACHED_CHUNKS)
            {
                var chunkNumberToRemove = _chunks.Keys.First();
                base.SetChunk(chunkNumberToRemove, _chunks[chunkNumberToRemove]);

                _chunks.TryRemove(chunkNumberToRemove, out _);
            }

            _chunks[chunkNumber] = cleartextChunk;
        }

        public override void Flush()
        {
            AssertNotDisposed();

            foreach (var chunk in _chunks)
            {
                chunkWriter.WriteChunk(chunk.Key, chunk.Value);
            }

            _chunks.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();

            _chunks.Clear();
        }
    }
}
