using System;
using System.Collections.Generic;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFileReceiver : IDisposable
    {
        private readonly Dictionary<ICiphertextPath, OpenCryptFile> _openCryptFiles;

        private readonly ISecurity _security;

        private readonly ChunkReceiverFactory _chunkReceiverFactory;

        private bool _disposed;

        public OpenCryptFileReceiver(ISecurity security, ChunkReceiverFactory chunkReceiverFactory)
        {
            this._security = security;
            this._chunkReceiverFactory = chunkReceiverFactory;

            this._openCryptFiles = new Dictionary<ICiphertextPath, OpenCryptFile>();
        }

        public OpenCryptFile GetOrCreate(ICiphertextPath ciphertextPath, IFileHeader fileHeader)
        {
            AssertNotDisposed();

            // TODO: It looks like a lot of openCryptFiles are created and closed - multiple streams and caching works fine but it benefits nothing because it can't be used that often.
            // Why? Because as mentioned before, openCryptFiles are quickly opened and closed thus preventing the cache to be used for longer and it constantly needs regenerating
            if (_openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile))
            {
                return openCryptFile;
            }

            var ciphertextStreamsManager = new CiphertextStreamsManager();
            //var chunkReceiver = GetChunkReceiverForCleartextFileStream(ciphertextStreamsManager, fileHeader);

            openCryptFile = new OpenCryptFile(ciphertextPath, () => GetChunkReceiverForCleartextFileStream(ciphertextStreamsManager, fileHeader), ciphertextStreamsManager, CloseCryptFile);
            _openCryptFiles.Add(ciphertextPath, openCryptFile);

            return openCryptFile;
        }

        private void CloseCryptFile(ICiphertextPath ciphertextPath)
        {
            if (_openCryptFiles.TryGetValue(ciphertextPath, out OpenCryptFile openCryptFile))
            {
                openCryptFile.Dispose();
                _openCryptFiles.Remove(ciphertextPath);
            }
        }

        private IChunkReceiver GetChunkReceiverForCleartextFileStream(CiphertextStreamsManager ciphertextStreamsManager, IFileHeader fileHeader)
        {
            var reader = _chunkReceiverFactory.GetChunkReader(_security, ciphertextStreamsManager, fileHeader);
            var writer = _chunkReceiverFactory.GetChunkWriter(_security, ciphertextStreamsManager, fileHeader);

            return _chunkReceiverFactory.GetChunkReceiver(reader, writer);
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

            _openCryptFiles.Values.DisposeCollection();
            _openCryptFiles.Clear();
        }
    }
}
