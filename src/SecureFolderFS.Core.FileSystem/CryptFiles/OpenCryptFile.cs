using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Represents an encrypting file opened on a file system.
    /// </summary>
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly Security _security;
        private readonly HeaderBuffer _headerBuffer;
        private readonly ChunkAccess _chunkAccess;
        private readonly StreamsManager _streamsManager;
        private readonly Action<string> _notifyClosed;
        private readonly Dictionary<Stream, long> _openedStreams;

        /// <summary>
        /// Gets the unique ID of the file.
        /// </summary>
        public string Id { get; }

        public OpenCryptFile(
            string id,
            Security security,
            HeaderBuffer headerBuffer,
            ChunkAccess chunkAccess,
            StreamsManager streamsManager,
            Action<string> notifyClosed)
        {
            Id = id;
            _security = security;
            _headerBuffer = headerBuffer;
            _chunkAccess = chunkAccess;
            _streamsManager = streamsManager;
            _notifyClosed = notifyClosed;
            _openedStreams = new();
        }

        /// <summary>
        /// Opens a new <see cref="PlaintextStream"/> on top of <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="ciphertextStream">The ciphertext stream to be wrapped by encrypting stream.</param>
        /// <returns>A new instance of <see cref="PlaintextStream"/>.</returns>
        public PlaintextStream OpenStream(Stream ciphertextStream)
        {
            // Register ciphertext stream
            if (_openedStreams.TryGetValue(ciphertextStream, out var value))
                _openedStreams[ciphertextStream] = ++value;
            else
                _openedStreams.Add(ciphertextStream, 1L);

            // Make sure to also add it to streams manager
            _streamsManager.AddStream(ciphertextStream);

            // Open the plaintext stream
            return new PlaintextStream(ciphertextStream, _security, _chunkAccess, _headerBuffer, NotifyClosed);
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
                _notifyClosed(Id);
                Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _streamsManager.Dispose();
            _openedStreams.Keys.DisposeElements();
            _openedStreams.Clear();
        }
    }
}
