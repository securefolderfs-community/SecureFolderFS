using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Cryptography
{
    /// <summary>
    /// Represents a security object used for encrypting and decrypting data in SecureFolderFS.
    /// </summary>
    public sealed class Security : IDisposable
    {
        private readonly SecretKey _encKey;
        private readonly SecretKey _macKey;

        // TODO: Needs docs
        public required IHeaderCrypt HeaderCrypt { get; init; }

        public required IContentCrypt ContentCrypt { get; init; }

        public required INameCrypt? NameCrypt { get; init; }

        private Security(SecretKey encKey, SecretKey macKey)
        {
            _encKey = encKey;
            _macKey = macKey;
        }

        public SecretKey CopyEncryptionKey()
        {
            return _encKey.CreateCopy();
        }

        public SecretKey CopyMacKey()
        {
            return _macKey.CreateCopy();
        }

        /// <summary>
        /// Creates a new instance of <see cref="Security"/> object that provides content encryption and cipher access.
        /// </summary>
        /// <param name="encKey">The DEK key that this class takes ownership of.</param>
        /// <param name="macKey">The MAC key that this class takes ownership of.</param>
        /// <param name="contentCipherId">The content cipher id.</param>
        /// <param name="fileNameCipherId">The file name cipher id.</param>
        /// <returns>A new <see cref="Security"/> object.</returns>
        public static Security CreateNew(SecretKey encKey, SecretKey macKey, string contentCipherId, string fileNameCipherId)
        {
            // Initialize crypt implementation
            IHeaderCrypt headerCrypt = contentCipherId switch
            {
                CipherId.AES_CTR_HMAC => new AesCtrHmacHeaderCrypt(encKey, macKey),
                CipherId.AES_GCM => new AesGcmHeaderCrypt(encKey, macKey),
                CipherId.XCHACHA20_POLY1305 => new XChaChaHeaderCrypt(encKey, macKey),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipherId))
            };
            IContentCrypt contentCrypt = contentCipherId switch
            {
                CipherId.AES_CTR_HMAC => new AesCtrHmacContentCrypt(macKey),
                CipherId.AES_GCM => new AesGcmContentCrypt(),
                CipherId.XCHACHA20_POLY1305 => new XChaChaContentCrypt(),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipherId))
            };
            INameCrypt? nameCrypt = fileNameCipherId switch
            {
                CipherId.AES_SIV => new AesSivNameCrypt(encKey, macKey),
                CipherId.NONE => null,
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameCipherId))
            };

            return new(encKey, macKey)
            {
                ContentCrypt = contentCrypt,
                HeaderCrypt = headerCrypt,
                NameCrypt = nameCrypt
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey.Dispose();
            _macKey.Dispose();

            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
            NameCrypt?.Dispose();
        }
    }
}
