using System;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides write access to chunks.
    /// </summary>
    public interface IChunkWriter : IDisposable
    {
        /// <summary>
        /// Writes <paramref name="cleartextChunk"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to write to.</param>
        /// <param name="cleartextChunk">The cleartext chunk to read from.</param>
        void WriteChunk(long chunkNumber, ReadOnlySpan<byte> cleartextChunk);
    }
}
