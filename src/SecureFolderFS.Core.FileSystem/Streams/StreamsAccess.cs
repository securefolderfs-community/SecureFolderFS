﻿using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    public sealed class StreamsAccess : IDisposable
    {
        private readonly Security _security;
        private readonly OpenCryptFileManager _cryptFileManager;

        private StreamsAccess(Security security, OpenCryptFileManager cryptFileManager)
        {
            _security = security;
            _cryptFileManager = cryptFileManager;
        }

        /// <summary>
        /// Opens a new plaintext stream wrapping <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <param name="ciphertextStream">The ciphertext stream to wrap by the plaintext stream.</param>
        /// <param name="takeFailureOwnership">Determines whether to close the <paramref name="ciphertextStream"/> when a new plaintext stream fails to open.</param>
        /// <returns>If successful, returns a new instance of plaintext <see cref="Stream"/>.</returns>
        public Stream OpenPlaintextStream(string id, Stream ciphertextStream, bool takeFailureOwnership = true)
        {
            try
            {
                // Get or create encrypted file from the file system
                var openCryptFile = _cryptFileManager.TryGet(id) ?? _cryptFileManager.NewCryptFile(id, new HeaderBuffer(_security.HeaderCrypt.HeaderPlaintextSize));

                // Open a new stream for that file registering existing ciphertext stream
                return openCryptFile.OpenStream(ciphertextStream);
            }
            catch (Exception)
            {
                if (takeFailureOwnership)
                    ciphertextStream.Dispose();

                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cryptFileManager.Dispose();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StreamsAccess"/> with the current implementation.
        /// </summary>
        /// <param name="security">The <see cref="Security"/> contract.</param>
        /// <param name="enableChunkCache">Determines if subsequent streams should cache chunks.</param>
        /// <param name="statistics">The <see cref="IFileSystemStatistics"/> to report statistics of opened streams.</param>
        /// <returns>A new instance of <see cref="StreamsAccess"/>.</returns>
        public static StreamsAccess CreateNew(Security security, bool enableChunkCache, IFileSystemStatistics statistics)
        {
            var cryptFileManager = new OpenCryptFileManager(security, enableChunkCache, statistics);
            return new StreamsAccess(security, cryptFileManager);
        }
    }
}
