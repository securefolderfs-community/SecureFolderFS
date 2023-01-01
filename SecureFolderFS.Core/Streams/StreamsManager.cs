using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureFolderFS.Core.Streams
{
    /// <inheritdoc cref="IStreamsManager"/>
    internal sealed class StreamsManager : IStreamsManager
    {
        private bool _disposed;
        private readonly List<Stream> _readOnlyStreams;
        private readonly List<Stream> _readWriteStreams;

        public StreamsManager()
        {
            _readOnlyStreams = new();
            _readWriteStreams = new();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Stream? GetReadOnlyStream()
        {
            if (_disposed)
                return null;

            lock (_readOnlyStreams)
            {
                return _readOnlyStreams.FirstOrDefault();
            }
        }

        /// <inheritdoc/>
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
                _readOnlyStreams.DisposeCollection();
                _readWriteStreams.DisposeCollection();

                _readOnlyStreams.Clear();
                _readWriteStreams.Clear();
            }
        }
    }
}
