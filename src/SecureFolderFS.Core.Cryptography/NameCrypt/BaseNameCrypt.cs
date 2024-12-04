using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal abstract class BaseNameCrypt : INameCrypt
    {
        protected readonly AesSiv128 aesSiv128;

        protected BaseNameCrypt(SecretKey encKey, SecretKey macKey)
        {
            aesSiv128 = AesSiv128.CreateInstance(encKey, macKey);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual string EncryptName(ReadOnlySpan<char> plaintextName, ReadOnlySpan<byte> directoryId)
        {
            // Allocate byte* for encoding
            var length = Encoding.UTF8.GetByteCount(plaintextName);
            var bytes = length < 256 ? stackalloc byte[length] : new byte[length];

            // Get bytes from plaintext name
            var count = Encoding.UTF8.GetBytes(plaintextName, bytes);

            // Encrypt
            var encryptedName = EncryptFileName(bytes.Slice(0, count), directoryId);

            // Encode with Base64Url
            return Base64Url.EncodeToString(encryptedName);
        }

        /// <inheritdoc/>
        public virtual string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            var ciphertextNameBuffer = Base64Url.DecodeFromChars(ciphertextName);
            var plaintextNameBuffer = DecryptFileName(ciphertextNameBuffer, directoryId);
            if (plaintextNameBuffer is null)
                return null;

            return Encoding.UTF8.GetString(plaintextNameBuffer);
        }

        protected abstract byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        protected abstract byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            aesSiv128.Dispose();
        }
    }
}
