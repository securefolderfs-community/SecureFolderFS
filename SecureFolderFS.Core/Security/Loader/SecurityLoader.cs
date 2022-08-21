using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;
using SecureFolderFS.Core.Chunks;

namespace SecureFolderFS.Core.Security.Loader
{
    internal sealed class SecurityLoader : ISecurityLoader
    {
        private readonly IChunkFactory _chunkFactory;

        public SecurityLoader(IChunkFactory chunkFactory)
        {
            _chunkFactory = chunkFactory;
        }

        public ISecurity LoadSecurity(BaseVaultConfiguration vaultConfiguration, ICipherProvider keyCryptor, MasterKey masterKeyCopy)
        {
            IFileContentCryptor fileContentCryptor;
            IFileHeaderCryptor fileHeaderCryptor;
            IFileNameCryptor fileNameCryptor;

            // IFileContentCryptor, IFileHeaderCryptor
            switch (vaultConfiguration.ContentCipherScheme)
            {
                case ContentCipherScheme.AES_CTR_HMAC:
                    {
                        fileContentCryptor = new AesCtrHmacContentCryptor(masterKeyCopy, keyCryptor, _chunkFactory);
                        fileHeaderCryptor = new AesCtrHmacHeaderCryptor(masterKeyCopy, keyCryptor);
                        break;
                    }

                case ContentCipherScheme.AES_GCM:
                    {
                        fileContentCryptor = new AesGcmContentCryptor(keyCryptor, _chunkFactory);
                        fileHeaderCryptor = new AesGcmHeaderCryptor(masterKeyCopy, keyCryptor);
                        break;
                    }

                case ContentCipherScheme.XChaCha20_Poly1305:
                    {
                        fileContentCryptor = new XChaCha20ContentCryptor(keyCryptor, _chunkFactory);
                        fileHeaderCryptor = new XChaCha20HeaderCryptor(masterKeyCopy, keyCryptor);
                        break;
                    }

                case ContentCipherScheme.Undefined:
                default:
                    throw new UndefinedCipherSchemeException(nameof(ContentCipherScheme));
            }

            // IFileNameCryptor
            switch (vaultConfiguration.FileNameCipherScheme)
            {
                case FileNameCipherScheme.AES_SIV:
                    fileNameCryptor = new AesSivNameCryptor(keyCryptor, masterKeyCopy);
                    break;

                case FileNameCipherScheme.None:
                    fileNameCryptor = null;
                    break;

                case FileNameCipherScheme.Undefined:
                default:
                    throw new UndefinedCipherSchemeException(nameof(FileNameCipherScheme));
            }

            ContentCrypt.IContentCryptor contentCryptor = new ContentCryptor(fileContentCryptor, fileHeaderCryptor, fileNameCryptor);

            Security security = new Security(contentCryptor, keyCryptor);
            return security;
        }
    }
}
