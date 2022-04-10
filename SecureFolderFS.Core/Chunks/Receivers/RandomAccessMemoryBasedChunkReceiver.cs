using System.Linq;
using System.Collections.Generic;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Chunks.Receivers
{
    internal sealed class RandomAccessMemoryBasedChunkReceiver : BaseChunkReceiver
    {
        private readonly Dictionary<long, ICleartextChunk> _chunks;

        public RandomAccessMemoryBasedChunkReceiver(IChunkReader chunkReader, IChunkWriter chunkWriter, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(chunkReader, chunkWriter, fileSystemStatsTracker)
        {
            this._chunks = new Dictionary<long, ICleartextChunk>();
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

                _chunks[chunkNumberToRemove].Dispose();
                _chunks.Remove(chunkNumberToRemove);
            }

            _chunks.AddOrReplace(chunkNumber, cleartextChunk);
        }

        public override void Flush()
        {
            AssertNotDisposed();

            foreach (var chunk in _chunks)
            {
                chunkWriter.WriteChunk(chunk.Key, chunk.Value);
                chunk.Value.Dispose();
            }
            _chunks.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();

            _chunks.Values.DisposeCollection();
            _chunks.Clear();
        }
    }
}
