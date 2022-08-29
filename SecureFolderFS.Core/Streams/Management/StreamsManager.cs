using System;
using System.Linq;
using System.Collections.Generic;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Streams.Management
{
    internal sealed class StreamsManager<TStream> : IDisposable
        where TStream : class, IDisposable
    {
        private readonly List<TStream> _streams;

        public StreamsManager()
        {
            _streams = new();
        }

        public void AddStream(TStream stream)
        {
            _streams.Add(stream);
        }

        public void RemoveStream(TStream stream)
        {
            _streams.Remove(stream);
        }

        public TStream? GetAvailableStream()
        {
            return _streams.FirstOrDefault();
        }

        public void Dispose()
        {
            _streams.DisposeCollection();
            _streams.Clear();
        }
    }
}
