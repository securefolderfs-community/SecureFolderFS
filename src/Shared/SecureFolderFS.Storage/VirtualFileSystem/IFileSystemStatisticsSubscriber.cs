using SecureFolderFS.Shared.Enums;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Extends <see cref="IFileSystemStatistics"/> with subscription capabilities to support multiple subscribers.
    /// </summary>
    public interface IFileSystemStatisticsSubscriber : IFileSystemStatistics
    {
        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.BytesRead"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToBytesRead(IProgress<long> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.BytesWritten"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToBytesWritten(IProgress<long> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.BytesEncrypted"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToBytesEncrypted(IProgress<long> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.BytesDecrypted"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToBytesDecrypted(IProgress<long> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.ChunkCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToChunkCache(IProgress<CacheAccessType> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.FileNameCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToFileNameCache(IProgress<CacheAccessType> progress);

        /// <summary>
        /// Subscribes to <see cref="IFileSystemStatistics.DirectoryIdCache"/> progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        IDisposable SubscribeToDirectoryIdCache(IProgress<CacheAccessType> progress);
    }
}

