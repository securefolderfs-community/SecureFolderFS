using System;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security
{
    internal interface ISecurity : IDisposable
    {
        IContentCryptor ContentCryptor { get; }

        IKeyCryptor KeyCryptor { get; }
    }
}
