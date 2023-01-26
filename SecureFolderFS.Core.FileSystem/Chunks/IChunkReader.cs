using System;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides read access to chunks.
    /// </summary>
    public interface IChunkReader : IDisposable
    {
        /// <summary>
        /// Reads chunk at specified <paramref name="chunkNumber"/> into <paramref name="cleartextChunk"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to read at.</param>
        /// <param name="cleartextChunk">The cleartext chunk to write to.</param>
        /// <returns>The amount of cleartext bytes or -1 if integrity error occurred.</returns>
        int ReadChunk(long chunkNumber, Span<byte> cleartextChunk);
    }
}
