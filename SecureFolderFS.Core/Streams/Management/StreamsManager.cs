using System;
using System.Linq;
using System.Collections.Generic;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Streams.Management
{
    internal sealed class StreamsManager<TStream> : IDisposable
        where TStream : IBaseFileStream
    {
        private readonly List<TStream> _streams;

        private bool _disposed;

        public StreamsManager()
        {
            this._streams = new List<TStream>();
        }

        public void AddStream(TStream stream)
        {
            AssertNotDisposed();

            _streams.Add(stream);
        }

        public void RemoveStream(TStream stream)
        {
            AssertNotDisposed();

            _streams.Remove(stream);
        }

        public TStream GetAvailableStream()
        {
            AssertNotDisposed();

            return _streams.FirstOrDefault() ?? throw new UnavailableStreamException();
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;

            _streams.DisposeCollection();
            _streams.Clear();
        }
    }
}
