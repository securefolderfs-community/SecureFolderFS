using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Sdk.Paths;
using System;
using System.IO;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal sealed class EncryptedUniformPathReceiver : BasePathReceiver, IPathReceiver
    {
        private readonly IDirectoryIdReceiver _directoryIdReceiver;
        private readonly IFileNameReceiver _fileNameReceiver;

        public EncryptedUniformPathReceiver(VaultPath vaultPath, IDirectoryIdReceiver directoryIdReceiver, IFileNameReceiver fileNameReceiver)
            : base(vaultPath)
        {
            _directoryIdReceiver = directoryIdReceiver;
            _fileNameReceiver = fileNameReceiver;
        }

        /// <inheritdoc/>
        public override string ToCiphertext(string cleartextPath)
        {
            return GetCorrectPath(cleartextPath, GetCiphertextFileName);
        }

        /// <inheritdoc/>
        public override string ToCleartext(string ciphertextPath)
        {
            return GetCorrectPath(ciphertextPath, GetCleartextFileName);
        }

        /// <inheritdoc/>
        public override string GetCleartextFileName(string cleartextFilePath)
        {
            var fileName = Path.GetFileName(cleartextFilePath);
            var directoryIdPath = PathHelpers.EnsurePathIsDirectoryIdOrGetFromParent(cleartextFilePath, vaultPath);
            var directoryId = _directoryIdReceiver.GetDirectoryId(directoryIdPath);

            return _fileNameReceiver.GetCleartextFileName(directoryId, fileName);
        }

        /// <inheritdoc/>
        public override string GetCiphertextFileName(string ciphertextFilePath)
        {
            var fileName = Path.GetFileName(ciphertextFilePath);
            var directoryIdPath = PathHelpers.EnsurePathIsDirectoryIdOrGetFromParent(ciphertextFilePath, vaultPath);
            var directoryId = _directoryIdReceiver.GetDirectoryId(directoryIdPath);

            return _fileNameReceiver.GetCiphertextFileName(directoryId, fileName);
        }

        private string GetCorrectPath(string path, Func<string, string> fileNameFunc)
        {
            var onlyPathAfterContent = path.Substring(vaultPath.VaultContentPath.Length, path.Length - vaultPath.VaultContentPath.Length);
            var correctPath = PathHelpers.EnsureTrailingPathSeparator(vaultPath.VaultContentPath);

            foreach (var fileName in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                correctPath += fileNameFunc(Path.Combine(correctPath, fileName)) + Path.PathSeparator;
            }

            return !path.EndsWith(Path.PathSeparator) ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }
    }
}
