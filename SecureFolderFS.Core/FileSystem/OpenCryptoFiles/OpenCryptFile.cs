using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly string _ciphertextPath;
        private readonly Action<string> _onCryptFileClosed;
        private readonly CiphertextStreamsManager _ciphertextStreamsManager;
        private readonly Dictionary<CleartextFileStream, long> _openedStreams;

        public CleartextHeaderBuffer FileHeader { get; }

        public IChunkAccess ChunkAccess { get; }

        public OpenCryptFile(string ciphertextPath, Action<string> onCryptFileClosed, CiphertextStreamsManager ciphertextStreamsManager, CleartextHeaderBuffer fileHeader, IChunkAccess chunkAccess)
        {
            _ciphertextPath = ciphertextPath;
            _onCryptFileClosed = onCryptFileClosed;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            FileHeader = fileHeader;
            ChunkAccess = chunkAccess;
            _openedStreams = new();
        }

        public void RegisterStream(CleartextFileStream cleartextFileStream, Stream ciphertextStream)
        {
            cleartextFileStream.StreamClosedCallback = CloseCallback;

            if (_openedStreams.ContainsKey(cleartextFileStream))
                _openedStreams[cleartextFileStream]++;
            else
                _openedStreams.Add(cleartextFileStream, 1L);

            _ciphertextStreamsManager.AddStream(ciphertextStream);
        }

        private void CloseCallback(CleartextFileStream cleartextFileStream)
        {
            try
            {
                if (_openedStreams.ContainsKey(cleartextFileStream) && --_openedStreams[cleartextFileStream] <= 0)
                    _openedStreams.Remove(cleartextFileStream);

                using var ciphertextStream = cleartextFileStream.CiphertextStream;
                _ciphertextStreamsManager.RemoveStream(ciphertextStream);
            }
            finally
            {
                if (_openedStreams.IsEmpty())
                    _onCryptFileClosed.Invoke(_ciphertextPath);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openedStreams.Keys.DisposeCollection();
            _openedStreams.Clear();
            _ciphertextStreamsManager.Dispose();
        }
    }
}
