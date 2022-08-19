using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly Dictionary<ICleartextFileStream, long> _openedCleartextStreams;

        private readonly ICiphertextPath _ciphertextPath;

        private readonly IChunkReceiver _chunkReceiver;

        private readonly CiphertextStreamsManager _ciphertextStreamsManager;

        private readonly Action<ICiphertextPath> _openCryptFileClosedCallback;

        private bool _disposed;

        public OpenCryptFile(ICiphertextPath ciphertextPath, IChunkReceiver chunkReceiver, CiphertextStreamsManager ciphertextStreamsManager, Action<ICiphertextPath> openCryptFileClosedCallback)
        {
            _ciphertextPath = ciphertextPath;
            _chunkReceiver = chunkReceiver;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            _openCryptFileClosedCallback = openCryptFileClosedCallback;

            _openedCleartextStreams = new Dictionary<ICleartextFileStream, long>();
        }

        public void Open(ICleartextFileStream cleartextFileStream, ICiphertextFileStream ciphertextFileStream)
        {
            AssertNotDisposed();

            FillCleartextFileStream(cleartextFileStream.AsCleartextFileStreamInternal());

            if (_openedCleartextStreams.ContainsKey(cleartextFileStream))
            {
                _openedCleartextStreams[cleartextFileStream]++;
            }
            else
            {
                _openedCleartextStreams.Add(cleartextFileStream, 1L);
            }

            _ciphertextStreamsManager.AddStream(ciphertextFileStream);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Close(ICleartextFileStream cleartextFileStream)
        {
            try
            {
                if (_openedCleartextStreams.ContainsKey(cleartextFileStream) && ((--_openedCleartextStreams[cleartextFileStream]) <= 0))
                {
                    _openedCleartextStreams.Remove(cleartextFileStream);

                    using var ciphertextFileStream = cleartextFileStream.AsCleartextFileStreamInternal().DangerousGetInternalCiphertextFileStream();
                    _ciphertextStreamsManager.RemoveStream(ciphertextFileStream);
                }
            }
            finally
            {
                if (_openedCleartextStreams.IsEmpty())
                {
                    _openCryptFileClosedCallback?.Invoke(_ciphertextPath);
                }
            }
        }

        public void FlushChunkReceiver()
        {
            //_chunkReceiver?.Flush();
        }

        private void FillCleartextFileStream(ICleartextFileStreamInternal cleartextFileStreamInternal)
        {
            cleartextFileStreamInternal.StreamClosedCallback = Close;
            cleartextFileStreamInternal.ChunkReceiver = _chunkReceiver;
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

            _openedCleartextStreams.Keys.DisposeCollection();
            _openedCleartextStreams.Clear();
        }
    }
}
