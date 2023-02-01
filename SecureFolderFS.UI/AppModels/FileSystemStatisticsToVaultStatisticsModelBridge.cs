using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.UI.AppModels
{
    internal sealed class FileSystemStatisticsToVaultStatisticsModelBridge : IVaultStatisticsModel, IFileSystemStatistics
    {
        private bool _disposed;
        private Action<long>? _readCallback;
        private Action<long>? _writeCallback;

        /// <inheritdoc/>
        public void NotifyForBytesRead(Action<long> callback)
        {
            if (_disposed)
                return;

            _readCallback = callback;
        }

        /// <inheritdoc/>
        public void NotifyForBytesWritten(Action<long> callback)
        {
            if (_disposed)
                return;

            _writeCallback = callback;
        }

        /// <inheritdoc/>
        public void NotifyBytesRead(long amount)
        {
            _readCallback?.Invoke(amount);
        }

        /// <inheritdoc/>
        public void NotifyBytesWritten(long amount)
        {
            _writeCallback?.Invoke(amount);
        }

        #region Unused Methods

        /// <inheritdoc/>
        public void NotifyBytesEncrypted(long amount)
        {
        }

        /// <inheritdoc/>
        public void NotifyBytesDecrypted(long amount)
        {
        }

        /// <inheritdoc/>
        public void NotifyChunkCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void NotifyChunkCacheHit()
        {
        }

        /// <inheritdoc/>
        public void NotifyChunkAccess()
        {
        }

        /// <inheritdoc/>
        public void NotifyDirectoryIdCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void NotifyDirectoryIdCacheHit()
        {
        }

        /// <inheritdoc/>
        public void NotifyDirectoryIdAccess()
        {
        }

        /// <inheritdoc/>
        public void NotifyFileNameCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void NotifyFileNameCacheHit()
        {
        }

        /// <inheritdoc/>
        public void NotifyFileNameAccess()
        {
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            _readCallback = null;
            _writeCallback = null;
        }
    }
}
