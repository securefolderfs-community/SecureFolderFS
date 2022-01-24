using System;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;

namespace SecureFolderFS.Core.Security.ContentCrypt
{
    internal interface IContentCryptor : IDisposable
    {
        IFileContentCryptor FileContentCryptor { get; }

        IFileHeaderCryptor FileHeaderCryptor { get; }

        IFileNameCryptor FileNameCryptor { get; }
    }
}
