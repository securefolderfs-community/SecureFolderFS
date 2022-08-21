using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security
{
    internal sealed class Security : ISecurity
    {
        public IContentCryptor ContentCryptor { get; }

        public ICipherProvider KeyCryptor { get; }

        public Security(IContentCryptor contentCryptor, ICipherProvider keyCryptor)
        {
            ContentCryptor = contentCryptor;
            KeyCryptor = keyCryptor;
        }

        public void Dispose()
        {
            ContentCryptor?.Dispose();
            KeyCryptor?.Dispose();
        }
    }
}
