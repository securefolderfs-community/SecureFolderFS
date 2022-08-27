using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal sealed class FileSystemOperations : IFileSystemOperations
    {
        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        public FileSystemOperations(IDirectoryIdReceiver directoryIdReceiver)
        {
            _directoryIdReceiver = directoryIdReceiver;
        }

        public bool InitializeDirectory(string ciphertextPath, bool skipExists)
        {
            if (skipExists || Directory.Exists(ciphertextPath))
            {
                var directoryId = _directoryIdReceiver.CreateNewDirectoryId();
                var directoryIdPath = PathHelpers.AppendDirectoryIdPath(ciphertextPath);

                using var fileStream = File.Create(directoryIdPath);
                fileStream.Write(directoryId.Id, 0, directoryId.Id.Length);

                return true;
            }

            return false;
        }

        public bool CanDeleteDirectory(string ciphertextPath)
        {
            var canDelete = true;
            using var directoryEnumerator = Directory.EnumerateFileSystemEntries(ciphertextPath).GetEnumerator();

            while (directoryEnumerator.MoveNext())
            {
                canDelete &= PathHelpers.IsCoreFile(Path.GetFileName(directoryEnumerator.Current));
                if (!canDelete)
                    break;
            }

            return canDelete;
        }

        public void PrepareDirectoryForDeletion(string ciphertextPath)
        {
            var directoryIdPath = PathHelpers.AppendDirectoryIdPath(ciphertextPath);
            _directoryIdReceiver.RemoveDirectoryId(directoryIdPath);
        }
    }
}