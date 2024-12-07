using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.CryptFiles;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Core.CryptFiles
{
    internal sealed class OpenCryptFileManager : IDisposable
    {
        private readonly Security _security;
        private readonly bool _enableChunkCache;
        private readonly IFileSystemStatistics _fileSystemStatistics;
        private readonly Dictionary<string, OpenCryptFile> _openCryptFiles;

        public OpenCryptFileManager(Security security, bool enableChunkCache, IFileSystemStatistics fileSystemStatistics)
        {
            _security = security;
            _enableChunkCache = enableChunkCache;
            _fileSystemStatistics = fileSystemStatistics;
            _openCryptFiles = new();
        }

        /// <summary>
        /// Tries to get a <see cref="OpenCryptFile"/> from opened files.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <returns>An instance of <see cref="OpenCryptFile"/>. The value may be null when the file is not present in opened files list.</returns>
        public OpenCryptFile? TryGet(string id)
        {
            lock (_openCryptFiles)
            {
                _openCryptFiles.TryGetValue(id, out var openCryptFile);
                return openCryptFile;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="OpenCryptFile"/>.
        /// </summary>
        /// <param name="id">The unique ID of the file.</param>
        /// <param name="headerBuffer">The plaintext header of the file.</param>
        /// <returns>If successful, returns an instance of <see cref="OpenCryptFile"/>.</returns>
        public OpenCryptFile NewCryptFile(string id, BufferHolder headerBuffer)
        {
            var cryptFile = GetCryptFile(id, headerBuffer);

            lock (_openCryptFiles)
                _openCryptFiles[id] = cryptFile;

            return cryptFile;
        }

        private OpenCryptFile GetCryptFile(string id, BufferHolder headerBuffer)
        {
            if (headerBuffer is not HeaderBuffer headerBuffer2)
                throw new ArgumentException($"{nameof(headerBuffer)} does not implement {nameof(HeaderBuffer)}.");

            var streamsManager = new StreamsManager();
            var chunkAccess = GetChunkAccess(streamsManager, headerBuffer2);

            return new OpenCryptFile(id, _security, headerBuffer2, chunkAccess, streamsManager, NotifyClosed);
        }

        private ChunkAccess GetChunkAccess(StreamsManager streamsManager, HeaderBuffer headerBuffer)
        {
            var chunkReader = new ChunkReader(_security, headerBuffer, streamsManager, _fileSystemStatistics);
            var chunkWriter = new ChunkWriter(_security, headerBuffer, streamsManager, _fileSystemStatistics);

            return _enableChunkCache
                ? new CachingChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics)
                : new ChunkAccess(chunkReader, chunkWriter, _security.ContentCrypt, _fileSystemStatistics);
        }

        private void NotifyClosed(string ciphertextPath)
        {
            lock (_openCryptFiles)
                _openCryptFiles.Remove(ciphertextPath);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_openCryptFiles)
            {
                _openCryptFiles.Values.DisposeElements();
                _openCryptFiles.Clear();
            }
        }
    }
}
