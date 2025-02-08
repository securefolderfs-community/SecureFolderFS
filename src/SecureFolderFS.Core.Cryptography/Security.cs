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
        /// <summary>
        /// Gets the key pair used for storing the DEK and MAC keys.
        /// </summary>
        public KeyPair KeyPair { get; }

        /// <summary>
        /// Gets or sets the header cryptography implementation.
        /// </summary>
        public required IHeaderCrypt HeaderCrypt { get; init; }

        /// <summary>
        /// Gets or sets the content cryptography implementation.
        /// </summary>
        public required IContentCrypt ContentCrypt { get; init; }

        /// <summary>
        /// Gets or sets the name cryptography implementation.
        /// </summary>
        public required INameCrypt? NameCrypt { get; init; }

        private Security(KeyPair keyPair)
        {
            KeyPair = keyPair;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Security"/> object that provides content encryption and cipher access.
        /// </summary>
        /// <param name="keyPair">The key pair that this class takes ownership of.</param>
        /// <param name="contentCipherId">The content cipher ID.</param>
        /// <param name="fileNameCipherId">The file name cipher ID.</param>
        /// <param name="fileNameEncodingId">The file name encoding ID.</param>
        /// <returns>A new <see cref="Security"/> object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid cipher ID is provided.</exception>
        public static Security CreateNew(KeyPair keyPair, string contentCipherId, string fileNameCipherId, string fileNameEncodingId)
        {
            // Initialize crypt implementation
            IHeaderCrypt headerCrypt = contentCipherId switch
            {
                CipherId.AES_CTR_HMAC => new AesCtrHmacHeaderCrypt(keyPair),
                CipherId.AES_GCM => new AesGcmHeaderCrypt(keyPair),
                CipherId.XCHACHA20_POLY1305 => new XChaChaHeaderCrypt(keyPair),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipherId))
            };
            IContentCrypt contentCrypt = contentCipherId switch
            {
                CipherId.AES_CTR_HMAC => new AesCtrHmacContentCrypt(keyPair.MacKey),
                CipherId.AES_GCM => new AesGcmContentCrypt(),
                CipherId.XCHACHA20_POLY1305 => new XChaChaContentCrypt(),
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipherId))
            };
            INameCrypt? nameCrypt = fileNameCipherId switch
            {
                CipherId.AES_SIV => new AesSivNameCrypt(keyPair, fileNameEncodingId),
                CipherId.NONE => null,
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameCipherId))
            };

            return new(keyPair)
            {
                ContentCrypt = contentCrypt,
                HeaderCrypt = headerCrypt,
                NameCrypt = nameCrypt
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            KeyPair.Dispose();
            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
            NameCrypt?.Dispose();
        }
    }
}
