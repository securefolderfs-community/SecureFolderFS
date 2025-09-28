using System;
using System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

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
        /// Opens a new plaintext stream wrapping <paramref name="wrappedStream"/>.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <param name="wrappedStream">The ciphertext stream to wrap by the plaintext stream.</param>
        /// <param name="takeFailureOwnership">Determines whether to close the <paramref name="wrappedStream"/> when a new plaintext stream fails to open.</param>
        /// <returns>If successful, returns a new instance of plaintext <see cref="Stream"/>.</returns>
        public Stream OpenPlaintextStream(string id, Stream wrappedStream, bool takeFailureOwnership = true)
        {
            try
            {
                // Get or create the encrypted file from the file system
                var openCryptFile = _cryptFileManager.TryGet(id) ?? _cryptFileManager.NewCryptFile(id, new HeaderBuffer(_security.HeaderCrypt.HeaderPlaintextSize));

                // Open a new stream for that file registering the existing ciphertext stream
                return openCryptFile.OpenStream(wrappedStream);
            }
            catch (Exception)
            {
                if (takeFailureOwnership)
                    wrappedStream.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Opens a new plaintext stream wrapping <paramref name="wrappedStream"/>.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <param name="wrappedStream">The ciphertext stream to wrap by the plaintext stream.</param>
        /// <param name="headerReadingStream">The optional Stream with read access to read the file header.</param>
        /// <param name="takeFailureOwnership">Determines whether to close the <paramref name="wrappedStream"/> when a new plaintext stream fails to open.</param>
        /// <returns>If successful, returns a new instance of plaintext <see cref="Stream"/>.</returns>
        /// <remarks>
        /// If the <paramref name="wrappedStream"/> does not support reading, the <paramref name="headerReadingStream"/>
        /// is used as long as the file header is not already present in cache.
        /// </remarks>
        public Stream OpenPlaintextStream(string id, Stream wrappedStream, Stream? headerReadingStream, bool takeFailureOwnership = true)
        {
            try
            {
                // Get or create the encrypted file from the file system
                var openCryptFile = _cryptFileManager.TryGet(id) ?? _cryptFileManager.NewCryptFile(id, new HeaderBuffer(_security.HeaderCrypt.HeaderPlaintextSize));

                // Check if the header is ready or if it can be read at a later time
                var headerBuffer = openCryptFile.HeaderBuffer;
                if (headerBuffer.IsHeaderReady || wrappedStream.CanRead)
                    return openCryptFile.OpenStream(wrappedStream);

                // Get the stream length
                var streamLength = wrappedStream.CanSeek ? wrappedStream.Length : headerReadingStream?.Length;
                _ = streamLength ?? throw new InvalidOperationException($"The {nameof(wrappedStream)} cannot seek and the {nameof(headerReadingStream)} was null.");

                // Check if there is data present in the stream (a possible file header)
                if (streamLength > 0)
                {
                    if (headerReadingStream is null)
                        throw new InvalidOperationException($"The {nameof(wrappedStream)} cannot read the header and the {nameof(headerReadingStream)} was null.");

                    // The header needs to be read at this point now as there is existing data in the wrapped stream,
                    // indicating overhead is present. Since the primary stream is write-only, we must
                    // use the dedicated read-only stream and store the header.
                    if (!headerBuffer.ReadHeader(headerReadingStream, _security))
                        throw new InvalidOperationException($"The {nameof(headerReadingStream)} cannot read the header.");

                    headerReadingStream.Dispose();
                }

                // Open a new stream for that file registering the existing ciphertext stream
                return openCryptFile.OpenStream(wrappedStream);
            }
            catch (Exception)
            {
                if (takeFailureOwnership)
                {
                    wrappedStream.Dispose();
                    headerReadingStream?.Dispose();
                }

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
