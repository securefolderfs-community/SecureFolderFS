using System;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security
{
    internal interface ISecurity : IDisposable
    {
        IContentCryptor ContentCryptor { get; }

        ICipherProvider KeyCryptor { get; }
    }
}
