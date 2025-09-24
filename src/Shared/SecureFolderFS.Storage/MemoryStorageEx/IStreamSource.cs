using System.IO;

namespace SecureFolderFS.Storage.MemoryStorageEx
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
    }
}
