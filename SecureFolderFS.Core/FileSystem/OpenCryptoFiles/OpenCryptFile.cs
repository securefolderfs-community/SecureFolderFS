using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly string _ciphertextPath;
        private readonly Action<string> _onCryptFileClosed;
        private readonly CiphertextStreamsManager _ciphertextStreamsManager;
        private readonly Dictionary<CleartextFileStream, long> _openedStreams;

        public IChunkAccess ChunkAccess { get; }

        public OpenCryptFile(string ciphertextPath, Action<string> onCryptFileClosed, CiphertextStreamsManager ciphertextStreamsManager, IChunkAccess chunkAccess)
        {
            _ciphertextPath = ciphertextPath;
            _onCryptFileClosed = onCryptFileClosed;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            ChunkAccess = chunkAccess;

            _openedStreams = new();
        }

        public void RegisterStream(CleartextFileStream cleartextFileStream, Stream ciphertextStream)
        {
            cleartextFileStream.StreamClosedCallback = Close;

            if (_openedStreams.ContainsKey(cleartextFileStream))
                _openedStreams[cleartextFileStream]++;
            else
                _openedStreams.Add(cleartextFileStream, 1L);

            _ciphertextStreamsManager.AddStream(ciphertextStream);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close(CleartextFileStream cleartextFileStream)
        {
            try
            {
                if (_openedStreams.ContainsKey(cleartextFileStream) && --_openedStreams[cleartextFileStream] <= 0)
                {
                    _openedStreams.Remove(cleartextFileStream);

                    using var ciphertextStream = cleartextFileStream.CiphertextStream;
                    _ciphertextStreamsManager.RemoveStream(ciphertextStream);
                }
            }
            finally
            {
                if (_openedStreams.IsEmpty())
                {
                    _onCryptFileClosed?.Invoke(_ciphertextPath);
                }
            }
        }

        public void FlushChunkReceiver()
        {
            Debugger.Break();
            ChunkAccess.Flush();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openedStreams.Keys.DisposeCollection();
            _openedStreams.Clear();
        }
    }
}
