using System;
using SecureFolderFS.Core.Paths.DirectoryMetadata;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileName
{
    internal interface IFileNameCryptor : IDisposable
    {
        string EncryptFileName(string cleartextName, DirectoryId directoryId);

        string DecryptFileName(string ciphertextFileName, DirectoryId directoryId);
    }
}
