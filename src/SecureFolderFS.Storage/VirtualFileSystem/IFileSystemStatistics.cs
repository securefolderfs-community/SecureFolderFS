using SecureFolderFS.Shared.Enums;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Allows for IO statistics to be reported.
    /// </summary>
    public interface IFileSystemStatistics
    {
        #region ReadWrite

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports the number of bytes read.
        /// </summary>
        IProgress<long>? BytesRead { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports the number of bytes written.
        /// </summary>
        IProgress<long>? BytesWritten { get; set; }

        #endregion

        #region Cryptography

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports the number of bytes encrypted.
        /// </summary>
        IProgress<long>? BytesEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports the number of bytes decrypted.
        /// </summary>
        IProgress<long>? BytesDecrypted { get; set; }

        #endregion

        #region Caching

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports on cache state of chunks.
        /// </summary>
        IProgress<CacheAccessType>? ChunkCache { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports on cache state of file names.
        /// </summary>
        IProgress<CacheAccessType>? FileNameCache { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports on cache state of directory IDs.
        /// </summary>
        IProgress<CacheAccessType>? DirectoryIdCache { get; set; }

        #endregion
    }
}
