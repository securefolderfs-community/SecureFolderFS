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
        private readonly ChunkAccess _chunkAccess;
        private readonly Action<string> _notifyClosed;
        private readonly StreamsManager _streamsManager;
        private readonly Dictionary<Stream, long> _openedStreams;

        /// <summary>
        /// Gets the unique ID of the file.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the buffer that holds the header information for the cryptographic file.
        /// </summary>
        public HeaderBuffer HeaderBuffer { get; }

        public OpenCryptFile(
            string id,
            Security security,
            HeaderBuffer headerBuffer,
            ChunkAccess chunkAccess,
            StreamsManager streamsManager,
            Action<string> notifyClosed)
        {
            Id = id;
            HeaderBuffer = headerBuffer;
            _security = security;
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
            // Register the ciphertext stream
            if (_openedStreams.TryGetValue(ciphertextStream, out var value))
                _openedStreams[ciphertextStream] = ++value;
            else
                _openedStreams.Add(ciphertextStream, 1L);

            // Make sure to also add it to streams manager
            _streamsManager.AddStream(ciphertextStream);

            // Open the plaintext stream
            return new PlaintextStream(ciphertextStream, _security, _chunkAccess, HeaderBuffer, NotifyClosed);
        }

        private void NotifyClosed(Stream ciphertextStream)
        {
            // Make sure to remove it and update the reference count
            if (_openedStreams.ContainsKey(ciphertextStream) && --_openedStreams[ciphertextStream] <= 0)
                _openedStreams.Remove(ciphertextStream);

            // Dispose of the stream
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
            _openedStreams.Keys.DisposeAll();
            _openedStreams.Clear();
        }
    }
}
