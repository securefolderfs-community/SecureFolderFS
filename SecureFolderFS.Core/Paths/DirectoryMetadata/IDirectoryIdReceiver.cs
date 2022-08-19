namespace SecureFolderFS.Core.Paths.DirectoryMetadata
{
    internal interface IDirectoryIdReceiver
    {
        DirectoryId GetDirectoryId(string ciphertextPath);

        DirectoryId CreateNewDirectoryId();

        void RemoveDirectoryId(string ciphertextPath);
    }
}
