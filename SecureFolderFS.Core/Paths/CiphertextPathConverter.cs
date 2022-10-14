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
            return GetCorrectPath(cleartextPath, GetCiphertextFileName);
        }

        /// <inheritdoc/>
        public string? ToCleartext(string ciphertextPath)
        {
            return GetCorrectPath(ciphertextPath, GetCleartextFileName);
        }

        /// <inheritdoc/>
        public string? GetCleartextFileName(string cleartextFilePath)
        {
            var fileName = Path.GetFileName(cleartextFilePath);
            var parentDirectory = Path.GetDirectoryName(cleartextFilePath);
            if (parentDirectory is null)
                return null;

            var directoryId = _directoryIdAccess.GetDirectoryId(parentDirectory);
            if (directoryId is null)
                return null;

            return _fileNameAccess.GetCleartextName(fileName, directoryId).ToString();
        }

        private string? GetCiphertextFileName(string ciphertextFilePath)
        {
            var fileName = Path.GetFileName(ciphertextFilePath);
            var parentDirectory = Path.GetDirectoryName(ciphertextFilePath);
            if (parentDirectory is null)
                return null;

            var directoryId = _directoryIdAccess.GetDirectoryId(parentDirectory);
            if (directoryId is null)
                return null;

            return _fileNameAccess.GetCiphertextName(fileName, directoryId).ToString();
        }

        // TODO: Refactor
        private string? GetCorrectPath(string path, Func<string, string?> fileNameFunc)
        {
            var onlyPathAfterContent = path.Substring(_vaultRootPath.Length, path.Length - _vaultRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(_vaultRootPath);

            foreach (var item in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = fileNameFunc(Path.Combine(correctPath, item));
                if (name is null)
                    return null;

                correctPath += $"{name}{Path.DirectorySeparatorChar}";
            }

            return !path.EndsWith(Path.DirectorySeparatorChar) ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }
    }
}
