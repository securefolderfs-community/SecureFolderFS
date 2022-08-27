namespace SecureFolderFS.Core.FileSystem.Operations
{
    internal interface IFileSystemOperations
    {
        bool InitializeDirectory(string ciphertextPath, bool skipExists);

        bool CanDeleteDirectory(string ciphertextPath);

        void PrepareDirectoryForDeletion(string ciphertextPath);
    }
}
