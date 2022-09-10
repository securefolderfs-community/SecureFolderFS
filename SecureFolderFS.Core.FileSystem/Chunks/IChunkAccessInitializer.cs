using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides access to initialization of <see cref="IChunkAccess"/>.
    /// </summary>
    public interface IChunkAccessInitializer
    {
        /// <summary>
        /// Initializes new instance of <see cref="IChunkReader"/> with specified <paramref name="streamManager"/>, and <paramref name="headerBuffer"/>.
        /// </summary>
        /// <param name="streamManager">The <see cref="IStreamsManager"/> used to retrieve streams for reading chunks.</param>
        /// <param name="headerBuffer">The cleartext header used for decryption.</param>
        /// <returns>A new instance of initialized <see cref="IChunkReader"/>.</returns>
        IChunkReader GetChunkReader(IStreamsManager streamManager, BufferHolder headerBuffer);

        /// <summary>
        /// Initializes new instance of <see cref="IChunkWriter"/> with specified <paramref name="streamManager"/>, and <paramref name="headerBuffer"/>.
        /// </summary>
        /// <param name="streamManager">The <see cref="IStreamsManager"/> used to retrieve streams for reading and writing chunks.</param>
        /// <param name="headerBuffer">The cleartext header used for decryption and encryption.</param>
        /// <returns>A new instance of initialized <see cref="IChunkWriter"/>.</returns>
        IChunkWriter GetChunkWriter(IStreamsManager streamManager, BufferHolder headerBuffer);

        /// <summary>
        /// Initializes new instance of <see cref="IChunkAccess"/> with specified <paramref name="chunkReader"/>, and <paramref name="chunkWriter"/>.
        /// </summary>
        /// <param name="chunkReader">The <see cref="IChunkReader"/> to use to read chunks.</param>
        /// <param name="chunkWriter">The <see cref="IChunkWriter"/> to use to write chunks.</param>
        /// <returns>A new instance of <see cref="IChunkAccess"/>.</returns>
        IChunkAccess GetChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter);
    }
}
