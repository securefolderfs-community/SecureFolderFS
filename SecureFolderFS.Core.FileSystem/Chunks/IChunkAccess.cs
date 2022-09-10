using System;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides access to cleartext chunks data in individual files.
    /// </summary>
    public interface IChunkAccess : IDisposable
    {
        /// <summary>
        /// Copies bytes from chunk into <paramref name="destination"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy from.</param>
        /// <param name="destination">The destination buffer to copy to.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying from.</param>
        /// <returns>The amount of bytes copied. Value is -1 if copy failed.</returns>
        int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk);

        /// <summary>
        /// Copies bytes from <paramref name="source"/> into chunk.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy to.</param>
        /// <param name="source">The source buffer to copy from.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying to.</param>
        /// <returns>The amount of bytes copied. Value is -1 if copy failed.</returns>
        int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk);

        /// <summary>
        /// Sets the length for specified chunk to <paramref name="length"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to set length to.</param>
        /// <param name="length">The length to trim.</param>
        void SetChunkLength(long chunkNumber, int length);

        /// <summary>
        /// Flushes outstanding chunks to disk.
        /// </summary>
        void Flush();
    }
}
