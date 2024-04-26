using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem.Statistics
{
    public interface IFileSystemStatistics : IReadWriteStatistics
    {
        #region Cryptography

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports the number of bytes encrypted.
        /// </summary>
        IProgress<long>? BytesEncrypted { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports the number of bytes decrypted.
        /// </summary>
        IProgress<long>? BytesDecrypted { get; set; }

        #endregion

        #region Caching

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports on cache state of chunks.
        /// </summary>
        IProgress<CacheAccessType>? ChunkCache { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports on cache state of file names.
        /// </summary>
        IProgress<CacheAccessType>? FileNameCache { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports on cache state of directory IDs.
        /// </summary>
        IProgress<CacheAccessType>? DirectoryIdCache { get; set; }

        #endregion
    }
}
