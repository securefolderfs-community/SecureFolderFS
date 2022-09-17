using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;

namespace SecureFolderFS.Core.Cryptography
{
    public interface ISecurity
    {
        ICipherProvider CipherProvider { get; }

        IContentCrypt ContentCrypt { get; }

        IHeaderCrypt HeaderCrypt { get; }

        IFileNameCryptor FileNameCryptor { get; }
    }
}
