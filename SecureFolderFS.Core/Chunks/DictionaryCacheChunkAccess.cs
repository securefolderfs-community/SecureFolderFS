using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.Sdk.Tracking;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class DictionaryCacheChunkAccess : BaseChunkAccess
    {
        private readonly ConcurrentDictionary<long, CleartextChunkBuffer> _chunkCache;

        public DictionaryCacheChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatsTracker? statsTracker)
            : base(chunkReader, chunkWriter, contentCrypt, statsTracker)
        {
            _chunkCache = new(3, Constants.IO.MAX_CACHED_CHUNKS);
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            // Get chunk
            var cleartextChunk = GetChunk(chunkNumber);
            if (cleartextChunk is null)
                return -1;

            // Copy from chunk
            var count = Math.Min(cleartextChunk.ActualLength - offsetInChunk, destination.Length);
            if (count < 0)
                return -1;

            cleartextChunk.Buffer.AsSpan(offsetInChunk, count).CopyTo(destination);

            return count;
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            // Get chunk
            var cleartextChunk = GetChunk(chunkNumber);
            if (cleartextChunk is null)
                return -1;

            // Update state of chunk
            cleartextChunk.IsDirty = true;

            // Copy to chunk
            var count = Math.Min(contentCrypt.ChunkCleartextSize - offsetInChunk, source.Length);
            if (count < 0)
                return -1;

            var destination = cleartextChunk.Buffer.AsSpan(offsetInChunk, count);
            source.Slice(0, count).CopyTo(destination);

            // Update actual length
            cleartextChunk.ActualLength = Math.Max(cleartextChunk.ActualLength, count + offsetInChunk);

            return count;
        }

        /// <inheritdoc/>
        public override void SetChunkLength(long chunkNumber, int length)
        {
            // Get chunk
            var cleartextChunk = GetChunk(chunkNumber);
            if (cleartextChunk is null)
                return;

            if (cleartextChunk.ActualLength > length)
            {
                cleartextChunk.ActualLength = length;
                cleartextChunk.IsDirty = true;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            foreach (var item in _chunkCache)
            {
                if (item.Value.IsDirty)
                    chunkWriter.WriteChunk(item.Key, item.Value.Buffer.AsSpan(0, item.Value.ActualLength));
            }

            _chunkCache.Clear();
        }

        private CleartextChunkBuffer? GetChunk(long chunkNumber)
        {
            if (!_chunkCache.TryGetValue(chunkNumber, out var cleartextChunk))
            {
                // Cache miss

                // Update stats
                statsTracker?.AddChunkAccess();
                statsTracker?.AddChunkCacheMiss();

                // Read chunk
                var buffer = new byte[contentCrypt.ChunkCleartextSize];
                var read = chunkReader.ReadChunk(chunkNumber, buffer);
                if (read < 0)
                    return null;

                // Create cleartext and set it to cache
                cleartextChunk = new CleartextChunkBuffer(buffer, read);
                SetChunk(chunkNumber, cleartextChunk);
            }
            else
            {
                // Cache hit

                // Update stats
                statsTracker?.AddChunkAccess();
                statsTracker?.AddChunkCacheHit();
            }

            return cleartextChunk;
        }

        private void SetChunk(long chunkNumber, CleartextChunkBuffer cleartextChunk)
        {
            if (_chunkCache.Count >= Constants.IO.MAX_CACHED_CHUNKS)
            {
                // Get chunk number to remove
                var chunkNumberToRemove = _chunkCache.Keys.First();

                // Write chunk
                if (_chunkCache.TryRemove(chunkNumberToRemove, out var removedChunk) && removedChunk.IsDirty)
                {
                    var realRemovedChunk = removedChunk.Buffer.AsSpan(0, removedChunk.ActualLength);
                    chunkWriter.WriteChunk(chunkNumberToRemove, realRemovedChunk);
                }
            }

            _chunkCache[chunkNumber] = cleartextChunk;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            _chunkCache.Clear();
        }
    }
}
