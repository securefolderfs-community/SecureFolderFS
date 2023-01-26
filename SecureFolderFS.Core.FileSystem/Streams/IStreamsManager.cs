using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <summary>
    /// Manages instances of <see cref="Stream"/>.
    /// </summary>
    public interface IStreamsManager : IDisposable
    {
        /// <summary>
        /// Adds <paramref name="stream"/> to the list of available streams.
        /// </summary>
        /// <remarks>
        /// The stream may be added to both read-only and read-write lists based on <see cref="Stream.CanWrite"/> property.
        /// </remarks>
        /// <param name="stream">The stream to add.</param>
        void AddStream(Stream stream);

        /// <summary>
        /// Removes <paramref name="stream"/> from the list of available streams.
        /// </summary>
        /// <remarks>
        /// The stream may be removed from both read-only and read-write lists based on <see cref="Stream.CanWrite"/> property.
        /// </remarks>
        /// <param name="stream">The stream to remove.</param>
        void RemoveStream(Stream stream);

        /// <summary>
        /// Gets a read-only stream instance from the list.
        /// </summary>
        /// <returns>An instance of <see cref="Stream"/>. The value may be null when no streams are available.</returns>
        Stream? GetReadOnlyStream();

        /// <summary>
        /// Gets a read-write stream instance from the list.
        /// </summary>
        /// <returns>An instance of <see cref="Stream"/>. The value may be null when no streams are available.</returns>
        Stream? GetReadWriteStream();
    }
}
