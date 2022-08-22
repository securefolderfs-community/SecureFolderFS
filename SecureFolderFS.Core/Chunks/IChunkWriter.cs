using System;

namespace SecureFolderFS.Core.Chunks
{
    /// <summary>
    /// Provides write access to chunks.
    /// </summary>
    internal interface IChunkWriter : IDisposable
    {
        /// <summary>
        /// Writes <paramref name="cleartextChunk"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to write to.</param>
        /// <param name="cleartextChunk">The cleartext chunk to read from.</param>
        void WriteChunk(long chunkNumber, ReadOnlySpan<byte> cleartextChunk);
    }
}
