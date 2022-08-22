using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;

namespace SecureFolderFS.Core.Security
{
    internal sealed class Security : ISecurity
    {
        public ICipherProvider CipherProvider { get; }

        public IContentCrypt ContentCrypt { get; }

        public IHeaderCrypt HeaderCrypt { get; }

        public IFileNameCryptor? FileNameCryptor { get; }

        public Security(ICipherProvider cipherProvider, IContentCrypt contentCrypt, IHeaderCrypt headerCrypt, IFileNameCryptor? fileNameCryptor)
        {
            CipherProvider = cipherProvider;
            ContentCrypt = contentCrypt;
            HeaderCrypt = headerCrypt;
            FileNameCryptor = fileNameCryptor;
        }

        public void Dispose()
        {
            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
            FileNameCryptor?.Dispose();
        }
    }
}
