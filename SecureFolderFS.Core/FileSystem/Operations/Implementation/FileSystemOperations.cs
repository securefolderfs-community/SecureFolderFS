using System.IO;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Paths.DirectoryMetadata;

namespace SecureFolderFS.Core.FileSystem.Operations.Implementation
{
    internal sealed class FileSystemOperations : IFileSystemOperations
    {
        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        public IFileOperations DangerousFileOperations { get; }

        public IDirectoryOperations DangerousDirectoryOperations { get; }

        public FileSystemOperations(IDirectoryIdReceiver directoryIdReceiver, IFileOperations fileOperations, IDirectoryOperations directoryOperations)
        {
            _directoryIdReceiver = directoryIdReceiver;
            DangerousFileOperations = fileOperations;
            DangerousDirectoryOperations = directoryOperations;
        }

        public bool InitializeWithDirectory(ICiphertextPath ciphertextPath)
        {
            return InitializeWithDirectory(ciphertextPath, false);
        }

        public bool InitializeWithDirectory(ICiphertextPath ciphertextPath, bool skipExists)
        {
            if (skipExists || DangerousDirectoryOperations.Exists(ciphertextPath.Path))
            {
                var directoryId = _directoryIdReceiver.CreateNewDirectoryId();
                var directoryIdPath = PathHelpers.AppendDirectoryIdPath(ciphertextPath.Path);

                using var fileStream = DangerousFileOperations.CreateFile(directoryIdPath);
                fileStream.Write(directoryId.Id, 0, directoryId.Id.Length);

                return true;
            }

            return false;
        }

        public bool CanDeleteDirectory(ICiphertextPath ciphertextPath)
        {
            var canDelete = true;
            using var directoryEnumerator = DangerousDirectoryOperations.EnumerateFileSystemEntries(ciphertextPath.Path).GetEnumerator();

            while (directoryEnumerator.MoveNext())
            {
                canDelete &= PathHelpers.IsCoreFile(Path.GetFileName(directoryEnumerator.Current));
                if (!canDelete)
                {
                    break;
                }
            }

            return canDelete;
        }

        public bool PrepareDirectoryForDeletion(ICiphertextPath ciphertextPath)
        {
            var directoryIdPath = PathHelpers.AppendDirectoryIdPath(ciphertextPath.Path);
            _directoryIdReceiver.RemoveDirectoryId(directoryIdPath);

            return true;
        }

        public bool PrepareFileForDeletion(ICiphertextPath ciphertextPath)
        {
            // TODO: Remove from filename cache

            return true;
        }

        public void MoveDirectory(ICiphertextPath sourceCiphertextPath, ICiphertextPath destinationCiphertextPath)
        {
            // TODO: Update cache
            DangerousDirectoryOperations.Move(sourceCiphertextPath.Path, destinationCiphertextPath.Path);
        }

        public void MoveFile(ICiphertextPath sourceCiphertextPath, ICiphertextPath destinationCiphertextPath)
        {
            // TODO: Update cache
            DangerousFileOperations.Move(sourceCiphertextPath.Path, destinationCiphertextPath.Path);
        }
    }
}