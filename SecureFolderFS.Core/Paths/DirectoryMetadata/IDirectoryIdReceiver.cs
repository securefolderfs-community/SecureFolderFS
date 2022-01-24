using System;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata
{
    internal interface IDirectoryIdReceiver : IDisposable
    {
        DirectoryId GetDirectoryId(string ciphertextPath);

        DirectoryId CreateNewDirectoryId();

        void RemoveDirectoryId(string ciphertextPath);
    }
}
