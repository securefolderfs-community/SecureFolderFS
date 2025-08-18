using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <summary>
    /// Manages instances of <see cref="Stream"/>.
    /// </summary>
    internal sealed class StreamsManager : IDisposable
    {
        private bool _disposed;
        private readonly List<Stream> _readOnlyStreams;
        private readonly List<Stream> _readWriteStreams;

        public StreamsManager()
        {
            _readOnlyStreams = new();
            _readWriteStreams = new();
        }

        /// <summary>
        /// Adds <paramref name="stream"/> to the list of available read-only and read-write streams.
        /// </summary>
        /// <remarks>
        /// The stream may be added to both read-only and read-write lists based on <see cref="Stream.CanWrite"/> property.
        /// </remarks>
        /// <param name="stream">The stream to add.</param>
        public void AddStream(Stream stream)
        {
            if (_disposed)
                return;

            lock (_readOnlyStreams)
            lock (_readWriteStreams)
            {
                _readOnlyStreams.Add(stream);

                if (stream.CanWrite)
                    _readWriteStreams.Add(stream);
            }
        }

        /// <summary>
        /// Removes <paramref name="stream"/> from the list of available read-only and read-write streams.
        /// </summary>
        /// <remarks>
        /// The stream may be removed from both read-only and read-write lists based on <see cref="Stream.CanWrite"/> property.
        /// </remarks>
        /// <param name="stream">The stream to remove.</param>
        public void RemoveStream(Stream stream)
        {
            if (_disposed)
                return;

            lock (_readOnlyStreams)
            lock (_readWriteStreams)
            {

                _ = _readOnlyStreams.Remove(stream);
                _ = _readWriteStreams.Remove(stream);
            }
        }

        /// <summary>
        /// Gets a read-only stream instance from the list.
        /// </summary>
        /// <returns>An instance of <see cref="Stream"/>. The value may be null when no streams are available.</returns>
        public Stream? GetReadOnlyStream()
        {
            if (_disposed)
                return null;

            lock (_readOnlyStreams)
            {
                return _readOnlyStreams.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a read-write stream instance from the list.
        /// </summary>
        /// <returns>An instance of <see cref="Stream"/>. The value may be null when no streams are available.</returns>
        public Stream? GetReadWriteStream()
        {
            if (_disposed)
                return null;

            lock (_readWriteStreams)
            {
                return _readWriteStreams.FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            lock (_readOnlyStreams)
            lock (_readWriteStreams)
            {
                _readOnlyStreams.DisposeAll();
                _readWriteStreams.DisposeAll();

                _readOnlyStreams.Clear();
                _readWriteStreams.Clear();
            }
        }
    }
}
