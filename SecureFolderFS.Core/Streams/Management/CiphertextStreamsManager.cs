using System;
using System.IO;

namespace SecureFolderFS.Core.Streams.Management
{
    internal sealed class CiphertextStreamsManager : IDisposable
    {
        private readonly StreamsManager<Stream> _readOnlyStreams;
        private readonly StreamsManager<Stream> _readWriteStreams;

        public CiphertextStreamsManager()
        {
            _readOnlyStreams = new();
            _readWriteStreams = new();
        }

        public void AddStream(Stream ciphertextStream)
        {
            _readOnlyStreams.AddStream(ciphertextStream);

            if (ciphertextStream.CanWrite)
                _readWriteStreams.AddStream(ciphertextStream);
        }

        public void RemoveStream(Stream ciphertextStream)
        {
            _readOnlyStreams.RemoveStream(ciphertextStream);
            _readWriteStreams.RemoveStream(ciphertextStream);
        }

        public Stream? GetReadOnlyStream()
        {
            return _readOnlyStreams.GetAvailableStream();
        }

        public Stream? GetReadWriteStream()
        {
            return _readWriteStreams.GetAvailableStream();
        }

        public void Dispose()
        {
            _readOnlyStreams.Dispose();
            _readWriteStreams.Dispose();
        }
    }
}
