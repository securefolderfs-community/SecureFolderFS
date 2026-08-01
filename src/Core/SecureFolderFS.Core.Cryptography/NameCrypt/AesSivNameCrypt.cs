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
            // The spans are passed straight through so the master keys never leave SecureKey's
            // protection boundary as ordinary, movable, never-zeroed heap arrays
            _aesSiv256 = keyPair.UseKeys(static (dekKey, macKey) => AesSiv256.CreateInstance(dekKey, macKey));
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
