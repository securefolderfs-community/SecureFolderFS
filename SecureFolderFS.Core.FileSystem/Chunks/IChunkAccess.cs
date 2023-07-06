using System;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides access to cleartext chunks data in individual files.
    /// </summary>
    public interface IChunkAccess : IDisposable
    {
        /// <summary>
        /// Copies bytes from chunk at specified <paramref name="chunkNumber"/> into <paramref name="destination"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy from.</param>
        /// <param name="destination">The destination buffer to copy to.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying from.</param>
        /// <returns>The amount of bytes copied. If successful, value is non-negative.</returns>
        int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk);

        /// <summary>
        /// Copies bytes from <paramref name="source"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy to.</param>
        /// <param name="source">The source buffer to copy from.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying to.</param>
        /// <returns>The amount of bytes copied. If successful, value is non-negative.</returns>
        int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk);

        /// <summary>
        /// Sets the length for specified chunk to <paramref name="length"/>.
        /// </summary>
        /// <param name="chunkNumber">The to chunk to modify at specified chunk number.</param>
        /// <param name="length">The length to extend or truncate to.</param>
        /// <param name="includeCurrentLength">Determines whether to include or exclude existing chunk length when resizing.</param>
        void SetChunkLength(long chunkNumber, int length, bool includeCurrentLength = false);

        /// <summary>
        /// Flushes outstanding chunks to disk.
        /// </summary>
        void Flush();
    }
}
