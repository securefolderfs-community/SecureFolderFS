using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    internal sealed class CiphertextPathConverter : IPathConverter
    {
        private readonly IFileNameAccess _fileNameAccess;
        private readonly DirectoryIdCache _directoryIdCache;

        /// <inheritdoc/>
        public string CiphertextRootPath { get; }

        public CiphertextPathConverter(string vaultRootPath, IFileNameAccess fileNameAccess, DirectoryIdCache directoryIdCache)
        {
            _fileNameAccess = fileNameAccess;
            _directoryIdCache = directoryIdCache;
            CiphertextRootPath = vaultRootPath;
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
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(ciphertextFilePath, _vaultRootPath);
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
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(cleartextFilePath, _vaultRootPath);
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
            var onlyPathAfterContent = rawPath.Substring(CiphertextRootPath.Length, rawPath.Length - CiphertextRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(CiphertextRootPath);
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
    }
}
