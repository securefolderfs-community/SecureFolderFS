using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Core.CryptFiles
{
    /// <inheritdoc cref="ICryptFile"/>
    internal sealed class OpenCryptFile : ICryptFile
    {
        private readonly Security _security;
        private readonly HeaderBuffer _headerBuffer;
        private readonly IChunkAccess _chunkAccess;
        private readonly IStreamsManager _streamsManager;
        private readonly Action<string> _notifyClosed;
        private readonly Dictionary<Stream, long> _openedStreams;

        /// <inheritdoc/>
        public string CiphertextPath { get; }

        public OpenCryptFile(
            string ciphertextPath,
            Security security,
            HeaderBuffer headerBuffer,
            IChunkAccess chunkAccess,
            IStreamsManager streamsManager,
            Action<string> notifyClosed)
        {
            CiphertextPath = ciphertextPath;
            _security = security;
            _headerBuffer = headerBuffer;
            _chunkAccess = chunkAccess;
            _streamsManager = streamsManager;
            _notifyClosed = notifyClosed;
            _openedStreams = new();
        }

        /// <inheritdoc/>
        public CleartextStream OpenStream(Stream ciphertextStream)
        {
            // Register ciphertext stream
            if (_openedStreams.ContainsKey(ciphertextStream))
                _openedStreams[ciphertextStream]++;
            else
                _openedStreams.Add(ciphertextStream, 1L);

            // Make sure to also add it to streams manager
            _streamsManager.AddStream(ciphertextStream);
            
            // Open the cleartext stream
            return new CleartextFileStream(ciphertextStream, _security, _chunkAccess, _headerBuffer, NotifyClosed);
        }

        private void NotifyClosed(Stream ciphertextStream)
        {
            // Make sure to remove it and update references count
            if (_openedStreams.ContainsKey(ciphertextStream) && --_openedStreams[ciphertextStream] <= 0)
                _openedStreams.Remove(ciphertextStream);

            // Dispose the stream
            _streamsManager.RemoveStream(ciphertextStream);
            ciphertextStream.Dispose();

            // Notify closed if no streams left
            if (_openedStreams.IsEmpty())
            {
                _notifyClosed(CiphertextPath);
                Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openedStreams.Keys.DisposeCollection();
            _openedStreams.Clear();
            _streamsManager.Dispose();
        }
    }
}
