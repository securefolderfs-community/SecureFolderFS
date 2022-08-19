namespace SecureFolderFS.Core.Paths.DirectoryMetadata.IO
{
    internal interface IDirectoryIdReader
    {
        DirectoryId ReadDirectoryId(string ciphertextPath); // TODO: Should this be ICiphertextPath?
    }
}
