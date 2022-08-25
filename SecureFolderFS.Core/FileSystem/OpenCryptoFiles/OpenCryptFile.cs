using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly Dictionary<ICleartextFileStream, long> _openedStreams;

        private readonly ICiphertextPath _ciphertextPath;

        private readonly IChunkAccess _chunkAccess;

        private readonly CiphertextStreamsManager _ciphertextStreamsManager;

        private readonly Action<ICiphertextPath> _openCryptFileClosedCallback;

        private bool _disposed;

        public IChunkAccess ChunkAccess { get; }

        public OpenCryptFile(ICiphertextPath ciphertextPath, IChunkAccess chunkAccess, CiphertextStreamsManager ciphertextStreamsManager, Action<ICiphertextPath> openCryptFileClosedCallback)
        {
            _ciphertextPath = ciphertextPath;
            _chunkAccess = chunkAccess;
            ChunkAccess = chunkAccess;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            _openCryptFileClosedCallback = openCryptFileClosedCallback;

            _openedStreams = new Dictionary<ICleartextFileStream, long>();
        }

        public void Open(ICleartextFileStream cleartextFileStream, ICiphertextFileStream ciphertextFileStream)
        {
            AssertNotDisposed();

            (cleartextFileStream as CleartextFileStream)!.StreamClosedCallback = Close;

            if (_openedStreams.ContainsKey(cleartextFileStream))
            {
                _openedStreams[cleartextFileStream]++;
            }
            else
            {
                _openedStreams.Add(cleartextFileStream, 1L);
            }

            _ciphertextStreamsManager.AddStream(ciphertextFileStream);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close(ICleartextFileStream cleartextFileStream)
        {
            try
            {
                if (_openedStreams.ContainsKey(cleartextFileStream) && ((--_openedStreams[cleartextFileStream]) <= 0))
                {
                    _openedStreams.Remove(cleartextFileStream);

                    using var ciphertextFileStream = cleartextFileStream.UnderlyingStream;
                    _ciphertextStreamsManager.RemoveStream(ciphertextFileStream);
                }
            }
            finally
            {
                if (_openedStreams.IsEmpty())
                {
                    _openCryptFileClosedCallback?.Invoke(_ciphertextPath);
                }
            }
        }

        public void FlushChunkReceiver()
        {
            //_chunkReceiver?.Flush();
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

            _openedStreams.Keys.DisposeCollection();
            _openedStreams.Clear();
        }
    }
}
