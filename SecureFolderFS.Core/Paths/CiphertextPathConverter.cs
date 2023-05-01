using SecureFolderFS.Core.FileSystem.Directories;
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
        private readonly string _vaultRootPath;
        private readonly IFileNameAccess _fileNameAccess;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        public CiphertextPathConverter(string vaultRootPath, IFileNameAccess fileNameAccess, IDirectoryIdAccess directoryIdAccess)
        {
            _vaultRootPath = vaultRootPath;
            _fileNameAccess = fileNameAccess;
            _directoryIdAccess = directoryIdAccess;
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
            // Construct directory ID path for the parent directory of ciphertextFilePath
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(ciphertextFilePath, _vaultRootPath);
            if (directoryIdPath is null)
                return null;

            // Allocate byte* for directory ID, or empty if path is root
            var directoryId = directoryIdPath.Length == 0 ? Span<byte>.Empty : stackalloc byte[FileSystem.Constants.DIRECTORY_ID_SIZE];

            // Get the directory ID
            if (directoryId.Length != 0 && !_directoryIdAccess.GetDirectoryId(directoryIdPath, directoryId))
                return null;

            // Get cleartext name
            return _fileNameAccess.GetCleartextName(Path.GetFileName(ciphertextFilePath), directoryId).ToString();
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public string? GetCiphertextFileName(string cleartextFilePath)
        {
            // Construct directory ID path for the parent directory of cleartextFilePath
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(cleartextFilePath, _vaultRootPath);
            if (directoryIdPath is null)
                return null;

            // Allocate byte* for directory ID, or empty if path is root
            var directoryId = directoryIdPath.Length == 0 ? Span<byte>.Empty : stackalloc byte[FileSystem.Constants.DIRECTORY_ID_SIZE];

            // Get the directory ID
            if (directoryId.Length != 0 && !_directoryIdAccess.GetDirectoryId(directoryIdPath, directoryId))
                return null;

            // Get ciphertext name
            return _fileNameAccess.GetCiphertextName(Path.GetFileName(cleartextFilePath), directoryId).ToString();
        }

        // TODO: Refactor
        private string? ConstructPath(string rawPath, bool convertToCiphertext)
        {
            var onlyPathAfterContent = rawPath.Substring(_vaultRootPath.Length, rawPath.Length - _vaultRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(_vaultRootPath);
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
