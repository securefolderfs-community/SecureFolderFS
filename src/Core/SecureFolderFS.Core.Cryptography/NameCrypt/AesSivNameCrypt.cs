using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal sealed class AesSivNameCrypt : BaseNameCrypt
    {
        private readonly AesSiv256 _aesSiv256;

        public AesSivNameCrypt(KeyPair keyPair, string fileNameEncodingId)
            : base(fileNameEncodingId)
        {
            _aesSiv256 = keyPair.UseKeys((dekKey, macKey) =>
            {
                // Note: AesSiv256 requires a byte[] key.
                return AesSiv256.CreateInstance(dekKey.ToArray(), macKey.ToArray());
            });
        }

        /// <inheritdoc/>
        protected override byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            return _aesSiv256.Encrypt(plaintextFileNameBuffer, directoryId);
        }

        /// <inheritdoc/>
        protected override byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId)
        {
            try
            {
                return _aesSiv256.Decrypt(ciphertextFileNameBuffer, directoryId);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _aesSiv256.Dispose();
        }
    }
}
