using Lex4K;
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
        protected readonly string fileNameEncodingId;

        protected BaseNameCrypt(SecretKey encKey, SecretKey macKey, string fileNameEncodingId)
        {
            this.aesSiv128 = AesSiv128.CreateInstance(encKey, macKey);
            this.fileNameEncodingId = fileNameEncodingId;
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
            var ciphertextNameBuffer = EncryptFileName(bytes.Slice(0, count), directoryId);

            // Encode string
            return Encode(ciphertextNameBuffer);
        }

        /// <inheritdoc/>
        public virtual string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            // Decode buffer
            var ciphertextNameBuffer = Decode(ciphertextName);

            // Decrypt
            var plaintextNameBuffer = DecryptFileName(ciphertextNameBuffer, directoryId);
            if (plaintextNameBuffer is null)
                return null;

            // Get string from plaintext buffer
            return Encoding.UTF8.GetString(plaintextNameBuffer);
        }

        protected abstract byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        protected abstract byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string Encode(ReadOnlySpan<byte> bytes)
        {
            return fileNameEncodingId switch
            {
                Constants.CipherId.ENCODING_BASE64URL => Base64Url.EncodeToString(bytes),
                Constants.CipherId.ENCODING_BASE4K => Base4K.EncodeChainToString(bytes),
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<byte> Decode(ReadOnlySpan<char> encoded)
        {
            return fileNameEncodingId switch
            {
                Constants.CipherId.ENCODING_BASE64URL => Base64Url.DecodeFromChars(encoded),
                Constants.CipherId.ENCODING_BASE4K => Base4K.DecodeChainToNewBuffer(encoded),
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
            };
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            aesSiv128.Dispose();
        }
    }
}
