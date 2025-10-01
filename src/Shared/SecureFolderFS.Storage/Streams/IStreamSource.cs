using System.IO;

namespace SecureFolderFS.Storage.Streams
{
    /// <summary>
    /// Represents an interface for creating streams.
    /// </summary>
    public interface IStreamSource
    {
        /// <summary>
        /// Creates and returns a new in-memory stream instance.
        /// </summary>
        /// <returns>A new <see cref="Stream"/> instance representing an in-memory stream.</returns>
        MemoryStream GetInMemoryStream();

        /// <summary>
        /// Wraps the provided data stream with the specified file access permissions and returns the resulting stream.
        /// </summary>
        /// <param name="access">The file access permissions to apply to the wrapped stream.</param>
        /// <param name="dataStream">The original data stream to be wrapped.</param>
        /// <returns>A <see cref="Stream"/> instance that represents the wrapped data stream.</returns>
        Stream WrapStreamSource(FileAccess access, Stream dataStream);
    }
}
