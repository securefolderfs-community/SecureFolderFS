using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal sealed class AesSivNameCrypt : BaseNameCrypt
    {
        public AesSivNameCrypt(SecretKey encKey, SecretKey macKey, CipherProvider cipherProvider)
            : base(encKey, macKey, cipherProvider)
        {
        }

        /// <inheritdoc/>
        protected override byte[] EncryptFileName(ReadOnlySpan<byte> cleartextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            return cipherProvider.AesSivCrypt.Encrypt(cleartextFileNameBuffer, encKey, macKey, directoryId);
        }

        /// <inheritdoc/>
        protected override byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            return cipherProvider.AesSivCrypt.Decrypt(ciphertextFileNameBuffer, encKey, macKey, directoryId);
        }
    }
}
