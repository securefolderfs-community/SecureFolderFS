using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Statistics;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    public sealed class CiphertextPathConverter : IPathConverter
    {
        private readonly FileNameAccess _fileNameAccess;
        private readonly DirectoryIdCache _directoryIdCache;

        /// <inheritdoc/>
        public string ContentRootPath { get; }

        private CiphertextPathConverter(string contentRoot, FileNameAccess fileNameAccess, DirectoryIdCache directoryIdCache)
        {
            _fileNameAccess = fileNameAccess;
            _directoryIdCache = directoryIdCache;
            ContentRootPath = contentRoot;
        }

        /// <inheritdoc/>
        public string? ToCiphertext(string cleartextPath)
        {
            return ConstructPath(cleartextPath, true);
        }

        /// <inheritdoc/>
        public string? ToCleartext(string ciphertextPath)
        {
            return ConstructPath(ciphertextPath, false);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public string? GetCleartextFileName(string ciphertextFilePath)
        {
            // Construct DirectoryID path for the parent directory of ciphertextFilePath
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(ciphertextFilePath, ContentRootPath);
            if (directoryIdPath is null)
                return null;

            // Allocate byte* for DirectoryID, or empty if path is root
            var directoryId = directoryIdPath.Length == 0 ? Span<byte>.Empty : stackalloc byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            if (directoryId.Length != 0 && !_directoryIdCache.GetDirectoryId(directoryIdPath, directoryId))
            {
                using var directoryIdStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var read = directoryIdStream.Read(directoryId);

                // Check the ID size
                if (read < FileSystem.Constants.DIRECTORY_ID_SIZE)
                {
                    // TODO: Report health status
                    return null;
                }

                // Set the DirectoryID to known IDs
                _directoryIdCache.SetDirectoryId(directoryIdPath, directoryId);
            }

            // Get cleartext name
            return _fileNameAccess.GetCleartextName(Path.GetFileName(ciphertextFilePath), directoryId);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public string? GetCiphertextFileName(string cleartextFilePath)
        {
            // Construct DirectoryID path for the parent directory of cleartextFilePath
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(cleartextFilePath, ContentRootPath);
            if (directoryIdPath is null)
                return null;

            // Allocate byte* for DirectoryID, or empty if path is root
            var directoryId = directoryIdPath.Length == 0 ? Span<byte>.Empty : stackalloc byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            if (directoryId.Length != 0 && !_directoryIdCache.GetDirectoryId(directoryIdPath, directoryId))
            {
                using var directoryIdStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var read = directoryIdStream.Read(directoryId);

                // Check the ID size
                if (read < FileSystem.Constants.DIRECTORY_ID_SIZE)
                {
                    // TODO: Report health status
                    return null;
                }

                // Set the DirectoryID to known IDs
                _directoryIdCache.SetDirectoryId(directoryIdPath, directoryId);
            }

            // Get ciphertext name
            return _fileNameAccess.GetCiphertextName(Path.GetFileName(cleartextFilePath), directoryId);
        }

        // TODO: Refactor
        private string? ConstructPath(string rawPath, bool convertToCiphertext)
        {
            var onlyPathAfterContent = rawPath.Substring(ContentRootPath.Length, rawPath.Length - ContentRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(ContentRootPath);
            var path = correctPath;

            foreach (var item in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = convertToCiphertext ? GetCiphertextFileName(Path.Combine(correctPath, item)) : GetCleartextFileName(Path.Combine(path, item));
                if (name is null)
                    return null;

                correctPath += $"{name}{Path.DirectorySeparatorChar}";
                path += $"{item}{Path.DirectorySeparatorChar}";
            }

            return !rawPath.EndsWith(Path.DirectorySeparatorChar) ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }

        /// <summary>
        /// Creates a new instance of <see cref="IPathConverter"/>.
        /// </summary>
        /// <param name="security">The <see cref="Security"/> contract.</param>
        /// <param name="contentRoot">The ciphertext storage root path.</param>
        /// <param name="enableFileNameCache">Determines if file paths should cache names.</param>
        /// <param name="directoryIdCache">The <see cref="DirectoryIdCache"/> that stored directory IDs.</param>
        /// <param name="statistics">The <see cref="IFileSystemStatistics"/> to report statistics of opened streams.</param>
        /// <returns>A new instance of <see cref="IPathConverter"/>.</returns>
        public static IPathConverter CreateNew(Security security, string contentRoot, DirectoryIdCache directoryIdCache, bool enableFileNameCache, IFileSystemStatistics statistics)
        {
            var fileNameAccess = enableFileNameCache
                ? new FileNameAccess(security, statistics)
                : new CachingFileNameAccess(security, statistics);

            return new CiphertextPathConverter(contentRoot, fileNameAccess, directoryIdCache);
        }
    }
}
