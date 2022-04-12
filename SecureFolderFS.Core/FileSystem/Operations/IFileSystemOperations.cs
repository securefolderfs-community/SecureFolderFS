using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal interface IFileSystemOperations
    {
        IFileOperations DangerousFileOperations { get; }

        IDirectoryOperations DangerousDirectoryOperations { get; }

        bool InitializeWithDirectory(ICiphertextPath ciphertextPath);

        bool InitializeWithDirectory(ICiphertextPath ciphertextPath, bool skipExists);

        bool CanDeleteDirectory(ICiphertextPath ciphertextPath);

        bool PrepareDirectoryForDeletion(ICiphertextPath ciphertextPath);

        bool PrepareFileForDeletion(ICiphertextPath ciphertextPath);

        void MoveFile(ICiphertextPath sourceCiphertextPath, ICiphertextPath destinationCiphertextPath);

        void MoveDirectory(ICiphertextPath sourceCiphertextPath, ICiphertextPath destinationCiphertextPath);
    }
}
