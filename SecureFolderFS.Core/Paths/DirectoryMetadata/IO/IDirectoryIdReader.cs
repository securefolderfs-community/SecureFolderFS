using System;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.IO
{
    internal interface IDirectoryIdReader : IDisposable
    {
        DirectoryId ReadDirectoryId(string ciphertextPath); // TODO: Should this be ICiphertextPath?
    }
}
