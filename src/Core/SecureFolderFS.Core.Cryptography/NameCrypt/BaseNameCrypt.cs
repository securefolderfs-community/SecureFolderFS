using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using Lex4K;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal abstract class BaseNameCrypt : INameCrypt
    {
        protected readonly string fileNameEncodingId;

        protected BaseNameCrypt(string fileNameEncodingId)
        {
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
            return fileNameEncodingId switch
            {
                Constants.CipherId.ENCODING_BASE64URL => Base64Url.EncodeToString(ciphertextNameBuffer),
                Constants.CipherId.ENCODING_BASE4K => Base4K.EncodeChainToString(ciphertextNameBuffer),
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
            };
        }

        /// <inheritdoc/>
        public virtual string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            try
            {
                // Decode buffer
                var ciphertextNameBuffer = fileNameEncodingId switch
                {
                    Constants.CipherId.ENCODING_BASE64URL => Base64Url.DecodeFromChars(ciphertextName),
                    Constants.CipherId.ENCODING_BASE4K => Base4K.DecodeChainToNewBuffer(ciphertextName),
                    _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
                };

                // Decrypt
                var plaintextNameBuffer = DecryptFileName(ciphertextNameBuffer, directoryId);
                if (plaintextNameBuffer is null)
                    return null;

                // Get string from plaintext buffer
                return Encoding.UTF8.GetString(plaintextNameBuffer);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected abstract byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        protected abstract byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
