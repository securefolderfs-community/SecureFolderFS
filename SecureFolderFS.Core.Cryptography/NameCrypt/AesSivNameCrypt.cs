using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal sealed class AesSivNameCrypt : BaseNameCrypt
    {
        public AesSivNameCrypt(SecretKey macKey, SecretKey encryptionKey, ICipherProvider cipherProvider)
            : base(macKey, encryptionKey, cipherProvider)
        {
        }

        /// <inheritdoc/>
        protected override void EncryptFileName(ReadOnlySpan<byte> cleartextFileNameBuffer, ReadOnlySpan<byte> directoryId, Span<byte> result)
        {
            cipherProvider.AesSivCrypt.Encrypt(cleartextFileNameBuffer, encryptionKey, macKey, directoryId, result);
        }

        /// <inheritdoc/>
        protected override bool DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId, Span<byte> result)
        {
            return cipherProvider.AesSivCrypt.Decrypt(ciphertextFileNameBuffer, encryptionKey, macKey, directoryId, result);
        }
    }
}
