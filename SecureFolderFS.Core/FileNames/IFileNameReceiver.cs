using SecureFolderFS.Core.Paths.DirectoryMetadata;

namespace SecureFolderFS.Core.FileNames
{
    internal interface IFileNameReceiver
    {
        string GetCleartextFileName(DirectoryId directoryId, string ciphertextFileName);

        void SetCleartextFileName(DirectoryId directoryId, string ciphertextFileName, string cleartextFileName);

        string GetCiphertextFileName(DirectoryId directoryId, string cleartextFileName);

        void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName);
    }
}
