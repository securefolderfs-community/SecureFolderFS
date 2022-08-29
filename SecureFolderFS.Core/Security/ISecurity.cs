using System;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;

namespace SecureFolderFS.Core.Security
{
    internal interface ISecurity : IDisposable
    { 
        ICipherProvider CipherProvider { get; }

        IContentCrypt ContentCrypt { get; }

        IHeaderCrypt HeaderCrypt { get; }

        IFileNameCryptor FileNameCryptor { get; }
    }
}
