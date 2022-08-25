using System;
using System.Collections.Generic;
using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Core.Chunks.ChunkAccessImpl;

namespace SecureFolderFS.Core.FileSystem.OpenCryptoFiles
{
    internal sealed class OpenCryptFileReceiver : IDisposable
    {
        private readonly Dictionary<ICiphertextPath, OpenCryptFile> _openCryptFiles;

        private readonly ISecurity _security;

        private readonly ChunkAccessFactory _chunkReceiverFactory;

        public OpenCryptFileReceiver(ISecurity security, ChunkAccessFactory chunkAccessFactory)
        {
            _security = security;
            _chunkReceiverFactory = chunkAccessFactory;

            _openCryptFiles = new Dictionary<ICiphertextPath, OpenCryptFile>();
        }

        public OpenCryptFile GetOrCreate(ICiphertextPath ciphertextPath, CleartextHeaderBuffer fileHeader)
        {
            // TODO: It looks like a lot of openCryptFiles are created and closed - multiple streams and caching works fine but it benefits nothing because it can't be used that often.
            // Why? Because as mentioned before, openCryptFiles are quickly opened and closed thus preventing the cache to be used for longer and it constantly needs regenerating
            if (_openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile))
            {
                return openCryptFile;
            }

            var ciphertextStreamsManager = new CiphertextStreamsManager();
            var chunkReceiver = GetChunkReceiverForCleartextFileStream(ciphertextStreamsManager, fileHeader);

            openCryptFile = new OpenCryptFile(ciphertextPath, chunkReceiver, ciphertextStreamsManager, CloseCryptFile);
            _openCryptFiles.Add(ciphertextPath, openCryptFile);

            return openCryptFile;
        }

        private void CloseCryptFile(ICiphertextPath ciphertextPath)
        {
            if (_openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile))
            {
                openCryptFile?.Dispose();
                _openCryptFiles.Remove(ciphertextPath);
            }
        }

        private IChunkAccess GetChunkReceiverForCleartextFileStream(CiphertextStreamsManager ciphertextStreamsManager, CleartextHeaderBuffer fileHeader)
        {
            var reader = _chunkReceiverFactory.GetChunkReader(ciphertextStreamsManager, fileHeader);
            var writer = _chunkReceiverFactory.GetChunkWriter(ciphertextStreamsManager, fileHeader);

            return _chunkReceiverFactory.GetChunkAccess(reader, writer);
        }

        public void Dispose()
        {
            _openCryptFiles.Values.DisposeCollection();
            _openCryptFiles.Clear();
        }
    }
}
