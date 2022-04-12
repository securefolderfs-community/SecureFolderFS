using System;
using System.IO;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal sealed class EncryptedUniformPathReceiver : BasePathReceiver, IPathReceiver
    {
        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        private readonly IFileNameReceiver _fileNameReceiver;

        public EncryptedUniformPathReceiver(VaultPath vaultPath, IDirectoryIdReceiver directoryIdReceiver, IFileNameReceiver fileNameReceiver)
            : base(vaultPath)
        {
            this._directoryIdReceiver = directoryIdReceiver;
            this._fileNameReceiver = fileNameReceiver;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override ICiphertextPath CiphertextPathFromRawCleartextPath(string cleartextPath)
        {
            return new CiphertextPath(GetCorrectPath(cleartextPath, GetCiphertextFileName));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override ICleartextPath CleartextPathFromRawCiphertextPath(string ciphertextPath)
        {
            return new CleartextPath(GetCorrectPath(ciphertextPath, GetCleartextFileName));
        }

        public override string GetCleartextFileName(string cleartextFilePath)
        {
            var fileName = Path.GetFileName(cleartextFilePath);
            var directoryIdPath = PathHelpers.EnsurePathIsDirectoryIdOrGetFromParent(cleartextFilePath, vaultPath);
            var directoryId = _directoryIdReceiver.GetDirectoryId(directoryIdPath);

            return _fileNameReceiver.GetCleartextFileName(directoryId, fileName);
        }

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

            foreach (var filename in onlyPathAfterContent.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                correctPath += fileNameFunc(Path.Combine(correctPath, filename)) + '\\';
            }

            return !path.EndsWith('\\') ? PathHelpers.EnsureNoTrailingPathSeparator(correctPath) : correctPath;
        }

        public override void Dispose()
        {
            this._directoryIdReceiver.Dispose();
        }
    }
}
