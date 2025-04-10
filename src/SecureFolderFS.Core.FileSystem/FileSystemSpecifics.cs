using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem
{
    /// <summary>
    /// Represents the specifics of the file system, including different classes to manipulate the file system components.
    /// </summary>
    public sealed class FileSystemSpecifics : IDisposable
    {
        /// <summary>
        /// Gets the root content folder that holds encrypted files.
        /// </summary>
        public required IFolder ContentFolder { get; init; }

        /// <summary>
        /// Gets the security object used for encrypting and decrypting data.
        /// </summary>
        public required Security Security { get; init; }

        /// <summary>
        /// Gets the streams access object used for managing file streams.
        /// </summary>
        public required StreamsAccess StreamsAccess { get; init; }

        /// <summary>
        /// Gets the file system options.
        /// </summary>
        public required FileSystemOptions Options { get; init; }

        /// <summary>
        /// Gets the cache for Directory IDs.
        /// </summary>
        public required UniversalCache<string, BufferHolder> DirectoryIdCache { get; init; }

        /// <summary>
        /// Gets the cache for ciphertext file names.
        /// </summary>
        public required UniversalCache<NameWithDirectoryId, string> CiphertextFileNameCache { get; init; }

        /// <summary>
        /// Gets the cache for plaintext file names.
        /// </summary>
        public required UniversalCache<NameWithDirectoryId, string> PlaintextFileNameCache { get; init; }

        private FileSystemSpecifics()
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            PlaintextFileNameCache.Dispose();
            CiphertextFileNameCache.Dispose();
            DirectoryIdCache.Dispose();
            StreamsAccess.Dispose();
            Security.Dispose();
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileSystemSpecifics"/>.
        /// </summary>
        /// <param name="security">The security object used for encrypting and decrypting data.</param>
        /// <param name="contentFolder">The root content folder that holds encrypted files.</param>
        /// <param name="options">The file system options.</param>
        /// <returns>A new instance of <see cref="FileSystemSpecifics"/>.</returns>
        public static FileSystemSpecifics CreateNew(Security security, IFolder contentFolder, FileSystemOptions options)
        {
            return new()
            {
                ContentFolder = contentFolder,
                PlaintextFileNameCache = options.IsCachingFileNames
                    ? new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_PLAINTEXT_FILENAMES, options.FileSystemStatistics.FileNameCache)
                    : new(false, options.FileSystemStatistics.FileNameCache),
                CiphertextFileNameCache = options.IsCachingFileNames
                    ? new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES, options.FileSystemStatistics.FileNameCache)
                    : new(false, options.FileSystemStatistics.FileNameCache),
                DirectoryIdCache = options.IsCachingDirectoryIds
                    ? new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_DIRECTORY_ID, options.FileSystemStatistics.DirectoryIdCache)
                    : new(false, options.FileSystemStatistics.DirectoryIdCache),
                Options = options,
                StreamsAccess = StreamsAccess.CreateNew(security, options.IsCachingChunks, options.FileSystemStatistics),
                Security = security
            };
        }
    }
}
