namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal interface IFileSystemOperations
    {
        void InitializeDirectory(string ciphertextPath);

        bool CanDeleteDirectory(string ciphertextPath);

        void PrepareDirectoryForDeletion(string ciphertextPath);
    }
}
