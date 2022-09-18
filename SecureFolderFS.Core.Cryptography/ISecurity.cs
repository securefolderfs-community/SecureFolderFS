using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using System;

namespace SecureFolderFS.Core.Cryptography
{
    // TODO: Needs docs
    public interface ISecurity : IDisposable
    {
        ICipherProvider CipherProvider { get; }

        IContentCrypt ContentCrypt { get; }

        IHeaderCrypt HeaderCrypt { get; }

        INameCrypt? NameCrypt { get; }
    }
}
