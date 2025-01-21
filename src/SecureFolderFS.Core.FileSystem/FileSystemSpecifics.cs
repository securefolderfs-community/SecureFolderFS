using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem
{
    public sealed class FileSystemSpecifics : IDisposable
    {
        /// <summary>
        /// Gets the root content folder that holds encrypted files.
        /// </summary>
        public required IFolder ContentFolder { get; init; }

        public required Security Security { get; init; }

        public required StreamsAccess StreamsAccess { get; init; }

        public required FileSystemOptions Options { get; init; }

        public required UniversalCache<string, BufferHolder> DirectoryIdCache { get; init; }
        
        public required UniversalCache<NameWithDirectoryId, string> CiphertextFileNameCache { get; init; }

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

        public static FileSystemSpecifics CreateNew(Security security, IFolder contentFolder, FileSystemOptions options)
        {
            return new()
            {
                ContentFolder = contentFolder,
                PlaintextFileNameCache = options.IsCachingFileNames
                    ? new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_Plaintext_FILENAMES, options.FileSystemStatistics.FileNameCache)
                    : new(false, options.FileSystemStatistics.FileNameCache),
                CiphertextFileNameCache = options.IsCachingFileNames
                    ? new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES, options.FileSystemStatistics.FileNameCache)
                    : new(false, options.FileSystemStatistics.FileNameCache),
                DirectoryIdCache = new(true, options.FileSystemStatistics.DirectoryIdCache),
                Options = options,
                StreamsAccess = StreamsAccess.CreateNew(security, options.IsCachingChunks, options.FileSystemStatistics),
                Security = security
            };
        }
    }
}
