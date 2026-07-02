using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <inheritdoc cref="ChunkAccess"/>
    internal sealed class CachingChunkAccess : ChunkAccess
    {
        private readonly OrderedDictionary<long, ChunkBuffer> _chunkCache;

        /// <inheritdoc/>
        public override bool FlushAvailable
        {
            get
            {
                // Hold the cache lock for the entire operation
                lock (_chunkCache)
                {
                    // Only chunks that were actually modified need flushing
                    foreach (var item in _chunkCache)
                    {
                        if (item.Value.WasModified)
                            return true;
                    }

                    return false;
                }
            }
        }

        public CachingChunkAccess(ChunkReader chunkReader, ChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatistics fileSystemStatistics)
            : base(chunkReader, chunkWriter, contentCrypt, fileSystemStatistics)
        {
            _chunkCache = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CHUNKS);
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                // Get chunk
                var plaintextChunk = GetChunk(chunkNumber);
                if (plaintextChunk is null)
                    return -1;

                // Copy from chunk
                var count = Math.Min(plaintextChunk.ActualLength - offsetInChunk, destination.Length);
                if (count < 0)
                    return -1;

                plaintextChunk.Buffer.AsSpan(offsetInChunk, count).CopyTo(destination);

                return count;
            }
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                // Get chunk
                var plaintextChunk = GetChunk(chunkNumber);
                if (plaintextChunk is null)
                    return -1;

                // Update state of chunk
                plaintextChunk.WasModified = true;

                // Copy to chunk
                var count = Math.Min(contentCrypt.ChunkPlaintextSize - offsetInChunk, source.Length);
                if (count < 0)
                    return -1;

                var destination = plaintextChunk.Buffer.AsSpan(offsetInChunk, count);
                source.Slice(0, count).CopyTo(destination);

                // Update actual length
                plaintextChunk.ActualLength = Math.Max(plaintextChunk.ActualLength, count + offsetInChunk);

                return count;
            }
        }

        /// <inheritdoc/>
        public override void SetChunkLength(long chunkNumber, int length, bool includeCurrentLength = false)
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                // Get chunk
                var plaintextChunk = GetChunk(chunkNumber);
                if (plaintextChunk is null)
                    return;

                // Add read length of existing chunk data to the full length if specified
                length += includeCurrentLength ? plaintextChunk.ActualLength : 0;
                length = Math.Max(length, 0);

                // Determine whether to extend or truncate the chunk
                if (length < plaintextChunk.ActualLength)
                {
                    // Truncate chunk
                    plaintextChunk.ActualLength = Math.Min(plaintextChunk.ActualLength, length);
                }
                else if (plaintextChunk.ActualLength < length)
                {
                    // Extend chunk
                    plaintextChunk.ActualLength = Math.Min(length, contentCrypt.ChunkPlaintextSize);
                }
                else
                    return; // Ignore resizing the same length

                plaintextChunk.WasModified = true;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                foreach (var item in _chunkCache)
                {
                    if (item.Value.WasModified)
                    {
                        chunkWriter.WriteChunk(item.Key, item.Value.Buffer.AsSpan(0, item.Value.ActualLength));

                        // Mark the chunk as clean so subsequent flushes don't rewrite it
                        item.Value.WasModified = false;
                    }
                }
            }
        }

        private ChunkBuffer? GetChunk(long chunkNumber)
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                if (!_chunkCache.TryGetValue(chunkNumber, out var plaintextChunk))
                {
                    // Cache miss, update stats
                    fileSystemStatistics.ChunkCache?.Report(CacheAccessType.CacheAccess);
                    fileSystemStatistics.ChunkCache?.Report(CacheAccessType.CacheMiss);

                    // Read chunk
                    var buffer = new byte[contentCrypt.ChunkPlaintextSize];
                    var read = chunkReader.ReadChunk(chunkNumber, buffer);
                    if (read < 0)
                        return null;

                    // Create plaintext and set it to cache
                    plaintextChunk = new ChunkBuffer(buffer, read);
                    SetChunk(chunkNumber, plaintextChunk);
                }
                else
                {
                    // Cache hit, update stats
                    fileSystemStatistics.ChunkCache?.Report(CacheAccessType.CacheAccess);
                    fileSystemStatistics.ChunkCache?.Report(CacheAccessType.CacheHit);
                }

                return plaintextChunk;
            }
        }

        private void SetChunk(long chunkNumber, ChunkBuffer plaintextChunk)
        {
            // Hold the cache lock for the entire operation
            lock (_chunkCache)
            {
                if (_chunkCache.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_CHUNKS)
                {
                    // Get chunk number to remove
                    var chunkNumberToRemove = _chunkCache.Keys.First();

                    // Write chunk
                    if (_chunkCache.Remove(chunkNumberToRemove, out var removedChunk) && removedChunk.WasModified)
                    {
                        var realRemovedChunk = removedChunk.Buffer.AsSpan(0, removedChunk.ActualLength);
                        chunkWriter.WriteChunk(chunkNumberToRemove, realRemovedChunk);
                    }
                }

                _chunkCache[chunkNumber] = plaintextChunk;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            lock (_chunkCache)
            {
                try
                {
                    // Flush outstanding modified chunks so data is not lost when
                    // the chunk access is disposed without a prior flush
                    Flush();
                }
                catch (Exception)
                {
                    // Dispose must not throw (the backing stream may already be unavailable)
                }

                base.Dispose();
                _chunkCache.Clear();
            }
        }
    }
}
