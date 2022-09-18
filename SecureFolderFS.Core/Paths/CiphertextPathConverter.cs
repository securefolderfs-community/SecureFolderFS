using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Helpers;
using System;
using System.IO;

namespace SecureFolderFS.Core.Paths
{
    /// <inheritdoc cref="IPathConverter"/>
    internal sealed class CiphertextPathConverter : BasePathConverter
    {
        private readonly IFileNameAccess _fileNameAccess;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        public CiphertextPathConverter(VaultPath vaultPath, IFileNameAccess fileNameAccess, IDirectoryIdAccess directoryIdAccess)
            : base(vaultPath)
        {
            _fileNameAccess = fileNameAccess;
            _directoryIdAccess = directoryIdAccess;
        }

        /// <inheritdoc/>
        public override string? ToCiphertext(string cleartextPath)
        {
            return GetCorrectPath(cleartextPath, GetCiphertextFileName);
        }

        /// <inheritdoc/>
        public override string? ToCleartext(string ciphertextPath)
        {
            return GetCorrectPath(ciphertextPath, GetCleartextFileName);
        }

        /// <inheritdoc/>
        public override string? GetCleartextFileName(string cleartextFilePath)
        {
            var fileName = Path.GetFileName(cleartextFilePath);
            var parentDirectory = Path.GetDirectoryName(cleartextFilePath);
            if (parentDirectory is null)
                return null;

            var directoryId = _directoryIdAccess.GetDirectoryId(parentDirectory);
            if (directoryId is null)
                return null;

            return _fileNameAccess.GetCleartextName(fileName, directoryId);
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

            return _fileNameAccess.GetCiphertextName(fileName, directoryId);
        }

        private string? GetCorrectPath(string path, Func<string, string?> fileNameFunc)
        {
            var onlyPathAfterContent = path.Substring(vaultRootPath.Length, path.Length - vaultRootPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(vaultRootPath);

            foreach (var fileName in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = fileNameFunc(Path.Combine(correctPath, fileName));
                if (name is null)
                    return null;

                correctPath += $"{name}{Path.DirectorySeparatorChar}";
            }

            return !path.EndsWith(Path.DirectorySeparatorChar) ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }
    }
}
