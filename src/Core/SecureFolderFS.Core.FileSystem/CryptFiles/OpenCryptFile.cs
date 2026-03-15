using System;
using System.Collections.Generic;
using System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <summary>
    /// Represents an encrypting file opened on a file system.
    /// </summary>
    internal sealed class OpenCryptFile : IDisposable
    {
        private readonly Security _security;
        private readonly Action<string> _notifyClosed;
        private readonly OpenCryptFileManager _cryptFileManager;
        private readonly Dictionary<Stream, (int RefCount, ChunkAccess ChunkAccess)> _openedStreams;

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
            OpenCryptFileManager cryptFileManager,
            Action<string> notifyClosed)
        {
            Id = id;
            HeaderBuffer = headerBuffer;
            _security = security;
            _cryptFileManager = cryptFileManager;
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
            if (_openedStreams.TryGetValue(ciphertextStream, out var existing))
                _openedStreams[ciphertextStream] = (existing.RefCount + 1, existing.ChunkAccess);
            else
            {
                var chunkAccess = _cryptFileManager.CreateChunkAccess(ciphertextStream, HeaderBuffer);
                _openedStreams.Add(ciphertextStream, (1, chunkAccess));
            }

            return new PlaintextStream(ciphertextStream, _security, _openedStreams[ciphertextStream].ChunkAccess, HeaderBuffer, NotifyClosed);
        }

        private void NotifyClosed(Stream ciphertextStream)
        {
            if (_openedStreams.TryGetValue(ciphertextStream, out var existing))
            {
                // Make sure to remove it and update the reference count
                if (--existing.RefCount <= 0)
                {
                    // Dispose of the stream
                    _openedStreams.Remove(ciphertextStream);
                    existing.ChunkAccess.Dispose();
                    ciphertextStream.Dispose();
                }
                else
                    _openedStreams[ciphertextStream] = (existing.RefCount, existing.ChunkAccess);
            }

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
            foreach (var (stream, (_, chunkAccess)) in _openedStreams)
            {
                chunkAccess.Dispose();
                stream.Dispose();
            }
            _openedStreams.Clear();
        }
    }
}