using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem.AppModels
{
    /// <inheritdoc cref="IFileSystemStatistics"/>
    public sealed class FileSystemStatistics : IFileSystemStatistics
    {
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
    }
}
