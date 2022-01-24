using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security
{
    internal sealed class Security : ISecurity
    {
        public IContentCryptor ContentCryptor { get; }

        public IKeyCryptor KeyCryptor { get; }

        public Security(IContentCryptor contentCryptor, IKeyCryptor keyCryptor)
        {
            this.ContentCryptor = contentCryptor;
            this.KeyCryptor = keyCryptor;
        }

        public void Dispose()
        {
            ContentCryptor?.Dispose();
            KeyCryptor?.Dispose();
        }
    }
}
