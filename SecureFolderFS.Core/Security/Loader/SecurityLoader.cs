using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.Security.Loader
{
    internal sealed class SecurityLoader : ISecurityLoader
    {
        public ISecurity LoadSecurity(BaseVaultConfiguration vaultConfiguration, ICipherProvider cipherProvider, MasterKey masterKeyCopy)
        {
            IContentCrypt contentCrypt;
            IHeaderCrypt headerCrypt;
            IFileNameCryptor? fileNameCryptor;

            // IContentCrypt, IHeaderCrypt
            switch (vaultConfiguration.ContentCipherScheme)
            {
                case ContentCipherScheme.AES_CTR_HMAC:
                    contentCrypt = new AesCtrHmacContentCrypt(masterKeyCopy.GetMacKey(), cipherProvider);
                    headerCrypt = new AesCtrHmacHeaderCrypt(masterKeyCopy, cipherProvider);
                    break;
                    

                case ContentCipherScheme.AES_GCM:
                    contentCrypt = new AesGcmContentCrypt(cipherProvider);
                    headerCrypt = new AesGcmHeaderCrypt(masterKeyCopy, cipherProvider);
                    break;

                case ContentCipherScheme.XChaCha20_Poly1305:
                    contentCrypt = new XChaChaContentCrypt(cipherProvider);
                    headerCrypt = new XChaChaHeaderCrypt(masterKeyCopy, cipherProvider);
                    break;

                case ContentCipherScheme.Undefined:
                default:
                    throw new UndefinedCipherSchemeException(nameof(ContentCipherScheme));
            }

            // IFileNameCryptor
            switch (vaultConfiguration.FileNameCipherScheme)
            {
                case FileNameCipherScheme.AES_SIV:
                    fileNameCryptor = new AesSivNameCryptor(cipherProvider, masterKeyCopy);
                    break;

                case FileNameCipherScheme.None:
                    fileNameCryptor = null;
                    break;

                case FileNameCipherScheme.Undefined:
                default:
                    throw new UndefinedCipherSchemeException(nameof(FileNameCipherScheme));
            }

            return new Security(cipherProvider, contentCrypt, headerCrypt, fileNameCryptor);
        }
    }
}
