using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem.AppModels
{
    /// <inheritdoc cref="IFileSystemStatistics"/>
    public sealed class FileSystemStatistics : IFileSystemStatisticsSubscriber, IDisposable
    {
        private readonly MulticastProgress<long> _bytesReadMulticast;
        private readonly MulticastProgress<long> _bytesWrittenMulticast;
        private readonly MulticastProgress<long> _bytesEncryptedMulticast;
        private readonly MulticastProgress<long> _bytesDecryptedMulticast;
        private readonly MulticastProgress<CacheAccessType> _chunkCacheMulticast;
        private readonly MulticastProgress<CacheAccessType> _fileNameCacheMulticast;
        private readonly MulticastProgress<CacheAccessType> _directoryIdCacheMulticast;

        public FileSystemStatistics()
        {
            _bytesReadMulticast = new MulticastProgress<long>();
            _bytesWrittenMulticast = new MulticastProgress<long>();
            _bytesEncryptedMulticast = new MulticastProgress<long>();
            _bytesDecryptedMulticast = new MulticastProgress<long>();
            _chunkCacheMulticast = new MulticastProgress<CacheAccessType>();
            _fileNameCacheMulticast = new MulticastProgress<CacheAccessType>();
            _directoryIdCacheMulticast = new MulticastProgress<CacheAccessType>();

            BytesRead = _bytesReadMulticast;
            BytesWritten = _bytesWrittenMulticast;
            BytesEncrypted = _bytesEncryptedMulticast;
            BytesDecrypted = _bytesDecryptedMulticast;
            ChunkCache = _chunkCacheMulticast;
            FileNameCache = _fileNameCacheMulticast;
            DirectoryIdCache = _directoryIdCacheMulticast;
        }

        /// <inheritdoc/>
        public IProgress<long>? BytesRead { get; set; }

        /// <inheritdoc/>
        public IProgress<long>? BytesWritten { get; set; }

        /// <inheritdoc/>
        public IProgress<long>? BytesEncrypted { get; set; }

        /// <inheritdoc/>
        public IProgress<long>? BytesDecrypted { get; set; }

        /// <inheritdoc/>
        public IProgress<CacheAccessType>? ChunkCache { get; set; }

        /// <inheritdoc/>
        public IProgress<CacheAccessType>? FileNameCache { get; set; }

        /// <inheritdoc/>
        public IProgress<CacheAccessType>? DirectoryIdCache { get; set; }

        /// <summary>
        /// Subscribes to <see cref="BytesRead"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToBytesRead(IProgress<long> progress) => _bytesReadMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="BytesWritten"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToBytesWritten(IProgress<long> progress) => _bytesWrittenMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="BytesEncrypted"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToBytesEncrypted(IProgress<long> progress) => _bytesEncryptedMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="BytesDecrypted"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToBytesDecrypted(IProgress<long> progress) => _bytesDecryptedMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="ChunkCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToChunkCache(IProgress<CacheAccessType> progress) => _chunkCacheMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="FileNameCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToFileNameCache(IProgress<CacheAccessType> progress) => _fileNameCacheMulticast.Subscribe(progress);

        /// <summary>
        /// Subscribes to <see cref="DirectoryIdCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable SubscribeToDirectoryIdCache(IProgress<CacheAccessType> progress) => _directoryIdCacheMulticast.Subscribe(progress);

        /// <inheritdoc/>
        public void Dispose()
        {
            _bytesReadMulticast.Dispose();
            _bytesWrittenMulticast.Dispose();
            _bytesEncryptedMulticast.Dispose();
            _bytesDecryptedMulticast.Dispose();
            _chunkCacheMulticast.Dispose();
            _fileNameCacheMulticast.Dispose();
            _directoryIdCacheMulticast.Dispose();
        }
    }
}
