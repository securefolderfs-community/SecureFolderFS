using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal sealed class AesSivNameCrypt : BaseNameCrypt
    {
        private readonly AesSiv128 _aesSiv128;

        public AesSivNameCrypt(KeyPair keyPair, string fileNameEncodingId)
            : base(fileNameEncodingId)
        {
            _aesSiv128 = keyPair.UseKeys((dekKey, macKey) =>
            {
                return AesSiv128.CreateInstance(dekKey.ToArray(), macKey.ToArray()); // Note: AesSiv128 requires a byte[] key.
            });
        }

        /// <inheritdoc/>
        protected override byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            return _aesSiv128.Encrypt(plaintextFileNameBuffer, directoryId);
        }

        /// <inheritdoc/>
        protected override byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            try
            {
                return _aesSiv128.Decrypt(ciphertextFileNameBuffer, directoryId);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _aesSiv128.Dispose();
        }
    }
}
