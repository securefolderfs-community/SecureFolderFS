using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.Cryptography
{
    /// <summary>
    /// Represents a security object used for encrypting and decrypting data in SecureFolderFS.
    /// </summary>
    public sealed class Security : IDisposable
    {
        // TODO: Needs docs
        // TODO: Add required modifier

        public CipherProvider CipherProvider { get; init; }

        public IHeaderCrypt HeaderCrypt { get; init; }

        public IContentCrypt ContentCrypt { get; init; }

        public INameCrypt? NameCrypt { get; init; }

        public static Security CreateNew(SecretKey encKey, SecretKey macKey, ContentCipherScheme contentCipher, FileNameCipherScheme fileNameCipher)
        {
            var cipherProvider = CipherProvider.CreateNew();

            // Initialize crypt implementation
            IHeaderCrypt headerCrypt = contentCipher switch
            {
                ContentCipherScheme.AES_CTR_HMAC => new AesCtrHmacHeaderCrypt(encKey, macKey, cipherProvider),
                ContentCipherScheme.AES_GCM => new AesGcmHeaderCrypt(encKey, macKey, cipherProvider),
                ContentCipherScheme.XChaCha20_Poly1305 => new XChaChaHeaderCrypt(encKey, macKey, cipherProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipher))
            };
            IContentCrypt contentCrypt = contentCipher switch
            {
                ContentCipherScheme.AES_CTR_HMAC => new AesCtrHmacContentCrypt(macKey, cipherProvider),
                ContentCipherScheme.AES_GCM => new AesGcmContentCrypt(cipherProvider),
                ContentCipherScheme.XChaCha20_Poly1305 => new XChaChaContentCrypt(cipherProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipher))
            };
            INameCrypt? nameCrypt = fileNameCipher switch
            {
                FileNameCipherScheme.AES_SIV => new AesSivNameCrypt(encKey, macKey, cipherProvider),
                FileNameCipherScheme.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameCipher))
            };

            return new()
            {
                CipherProvider = cipherProvider,
                ContentCrypt = contentCrypt,
                HeaderCrypt = headerCrypt,
                NameCrypt = nameCrypt
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
        }
    }
}
