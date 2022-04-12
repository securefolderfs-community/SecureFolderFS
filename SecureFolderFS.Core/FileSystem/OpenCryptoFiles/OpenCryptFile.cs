using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Sdk.Streams;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly Dictionary<ICleartextFileStream, long> _openedCleartextStreams;

        private readonly ICiphertextPath _ciphertextPath;

        // TODO: Make this instance, not Func! The reason it is Func right now, is that the Func will generate new instance each time it is called.
        // So in turn, new instance is created for each CleartextFileStream - which as you can guess the performance could be improved so
        // The cache is re-used between other streams. Unfortunately that doesn't work as good because it has some threading problems (?) and
        // so, some chunks become corrupted. I'll leave it as it is for now, but in the future, fix the threading issue and re-use IChunkReceiver.
        private readonly Func<IChunkReceiver> _chunkReceiver;

        private readonly CiphertextStreamsManager _ciphertextStreamsManager;

        private readonly Action<ICiphertextPath> _openCryptFileClosedCallback;

        private bool _disposed;

        public OpenCryptFile(ICiphertextPath ciphertextPath, Func<IChunkReceiver> chunkReceiver, CiphertextStreamsManager ciphertextStreamsManager, Action<ICiphertextPath> openCryptFileClosedCallback)
        {
            this._ciphertextPath = ciphertextPath;
            this._chunkReceiver = chunkReceiver;
            this._ciphertextStreamsManager = ciphertextStreamsManager;
            this._openCryptFileClosedCallback = openCryptFileClosedCallback;

            this._openedCleartextStreams = new Dictionary<ICleartextFileStream, long>();
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

                    using var ciphertextFileStream = cleartextFileStream.AsCleartextFileStreamInternal().GetInternalCiphertextFileStream();
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
            cleartextFileStreamInternal.ChunkReceiver = _chunkReceiver();
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
