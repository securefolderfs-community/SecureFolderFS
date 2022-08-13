using System;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.WinUI.AppModels
{
    internal sealed class FileSystemStatsTrackerToVaultStatisticsModelBridge : IVaultStatisticsModel, IFileSystemStatsTracker
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
        public void NotifyForBytesWrite(Action<long> callback)
        {
            if (_disposed)
                return;

            _writeCallback = callback;
        }

        /// <inheritdoc/>
        public void AddBytesRead(long amount)
        {
            _readCallback?.Invoke(amount);
        }

        /// <inheritdoc/>
        public void AddBytesWritten(long amount)
        {
            _writeCallback?.Invoke(amount);
        }

        #region Unused Methods

        /// <inheritdoc/>
        public void AddBytesEncrypted(long amount)
        {
        }

        /// <inheritdoc/>
        public void AddBytesDecrypted(long amount)
        {
        }

        /// <inheritdoc/>
        public void AddChunkCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void AddChunkCacheHit()
        {
        }

        /// <inheritdoc/>
        public void AddChunkAccess()
        {
        }

        /// <inheritdoc/>
        public void AddDirectoryIdCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void AddDirectoryIdCacheHit()
        {
        }

        /// <inheritdoc/>
        public void AddDirectoryIdAccess()
        {
        }

        /// <inheritdoc/>
        public void AddFileNameCacheMiss()
        {
        }

        /// <inheritdoc/>
        public void AddFileNameCacheHit()
        {
        }

        /// <inheritdoc/>
        public void AddFileNameAccess()
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
