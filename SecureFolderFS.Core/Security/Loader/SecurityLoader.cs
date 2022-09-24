using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.Security.Loader
{
    internal sealed class SecurityLoader
    {
        public ISecurity LoadSecurity(BaseVaultConfiguration vaultConfiguration, ICipherProvider cipherProvider, MasterKey masterKeyCopy)
        {
            IContentCrypt contentCrypt;
            IHeaderCrypt headerCrypt;
            INameCrypt? nameCrypt;

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
                    nameCrypt = new AesSivNameCryptor(cipherProvider, masterKeyCopy);
                    break;

                case FileNameCipherScheme.None:
                    nameCrypt = null;
                    break;

                case FileNameCipherScheme.Undefined:
                default:
                    throw new UndefinedCipherSchemeException(nameof(FileNameCipherScheme));
            }

            return new Security(cipherProvider, contentCrypt, headerCrypt, nameCrypt);
        }
    }
}
