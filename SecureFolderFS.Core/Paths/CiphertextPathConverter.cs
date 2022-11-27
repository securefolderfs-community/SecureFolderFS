using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.IO;

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
        public string? GetCleartextFileName(string cleartextFilePath)
        {
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(cleartextFilePath, _vaultRootPath);
            if (directoryIdPath is null)
                return null;

            var directoryId = _directoryIdAccess.GetDirectoryId(directoryIdPath);
            if (directoryId is null)
                return null;

            return _fileNameAccess.GetCleartextName(Path.GetFileName(cleartextFilePath), directoryId).ToString();
        }

        private string? GetCiphertextFileName(string ciphertextFilePath)
        {
            var directoryIdPath = PathHelpers.GetDirectoryIdPathOfParent(ciphertextFilePath, _vaultRootPath);
            if (directoryIdPath is null)
                return null;

            var directoryId = _directoryIdAccess.GetDirectoryId(directoryIdPath);
            if (directoryId is null)
                return null;

            return _fileNameAccess.GetCiphertextName(Path.GetFileName(ciphertextFilePath), directoryId).ToString();
        }

        // TODO: Refactor
        private string? ConstructPath(string rawPath, bool convertToCiphertext)
        {
            var onlyPathAfterContent = rawPath.Substring(_vaultRootPath.Length, rawPath.Length - _vaultRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(_vaultRootPath);

            foreach (var item in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = convertToCiphertext ? GetCiphertextFileName(Path.Combine(correctPath, item)) : GetCleartextFileName(Path.Combine(correctPath, item));
                if (name is null)
                    return null;

                correctPath += $"{name}{Path.DirectorySeparatorChar}";
            }

            return !rawPath.EndsWith(Path.DirectorySeparatorChar) ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }
    }
}
