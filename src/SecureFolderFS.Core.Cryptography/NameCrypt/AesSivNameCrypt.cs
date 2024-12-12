using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal sealed class AesSivNameCrypt : BaseNameCrypt
    {
        public AesSivNameCrypt(SecretKey encKey, SecretKey macKey, string fileNameEncodingId)
            : base(encKey, macKey, fileNameEncodingId)
        {
        }

        /// <inheritdoc/>
        protected override byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            return aesSiv128.Encrypt(plaintextFileNameBuffer, directoryId);
        }

        /// <inheritdoc/>
        protected override byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            try
            {
                return aesSiv128.Decrypt(ciphertextFileNameBuffer, directoryId);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
