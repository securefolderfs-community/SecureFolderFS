using System;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Sdk.Streams;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal interface IFileHeaderCryptor : IDisposable
    {
        int HeaderSize { get; }

        IFileHeader CreateFileHeader();

        byte[] EncryptHeader(IFileHeader fileHeader);

        IFileHeader DecryptHeader(byte[] ciphertextFileHeader);

        byte[] CiphertextHeaderFromCiphertextFileStream(ICiphertextFileStream ciphertextFileStream);
    }
}
