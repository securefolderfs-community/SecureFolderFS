using System;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using static SecureFolderFS.Core.Constants.Security.Headers.XChaCha20Poly1305;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Header.XChaChaHeaderExtensions;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class XChaChaHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderCleartextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public XChaChaHeaderCrypt(MasterKey masterKey, ICipherProvider cipherProvider)
            : base(masterKey, cipherProvider)
        {
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void CreateHeader(Span<byte> cleartextHeader)
        {
            // Nonce
            secureRandom.GetNonZeroBytes(cleartextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            secureRandom.GetBytes(cleartextHeader.Slice(HEADER_NONCE_SIZE));
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void EncryptHeader(ReadOnlySpan<byte> cleartextHeader, Span<byte> ciphertextHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

            // Nonce
            cleartextHeader.GetHeaderNonce().CopyTo(ciphertextHeader);

            // Encrypt
            cipherProvider.XChaCha20Poly1305Crypt.Encrypt(
                cleartextHeader.GetHeaderContentKey(),
                encKey,
                cleartextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE),
                default);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> cleartextHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(cleartextHeader);

            // Decrypt
            return cipherProvider.XChaCha20Poly1305Crypt.Decrypt(
                ciphertextHeader.Slice(HEADER_NONCE_SIZE),
                encKey,
                ciphertextHeader.GetHeaderNonce(),
                cleartextHeader.Slice(HEADER_NONCE_SIZE),
                default);
        }
    }
}
