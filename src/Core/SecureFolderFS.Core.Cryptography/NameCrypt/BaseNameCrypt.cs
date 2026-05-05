using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using SecureFolderFS.Core.Cryptography.Cipher;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal abstract class BaseNameCrypt : INameCrypt
    {
        protected const NormalizationForm NORMALIZATION = NormalizationForm.FormC;
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
                Constants.CipherId.ENCODING_BASE4K => SecombaBase4K.Encode(ciphertextNameBuffer),
                _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
            };
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            try
            {
                if (fileNameEncodingId == Constants.CipherId.ENCODING_BASE4K && !ciphertextName.IsNormalized(NORMALIZATION))
                {
                    var normalizedLength = ciphertextName.GetNormalizedLength(NORMALIZATION);
                    var destination = normalizedLength < 256 ? stackalloc char[normalizedLength] : new char[normalizedLength];

                    // Try to normalize
                    if (!ciphertextName.TryNormalize(destination, out var written, NORMALIZATION))
                        return null;

                    // Decode
                    return Decode(destination.Slice(0, written), directoryId);
                }

                // Skip normalization and decode directly
                return Decode(ciphertextName, directoryId);
            }
            catch (Exception)
            {
                return null;
            }

            string? Decode(ReadOnlySpan<char> name, ReadOnlySpan<byte> associatedData)
            {
                // Decode buffer
                var decoded = fileNameEncodingId switch
                {
                    Constants.CipherId.ENCODING_BASE64URL => Base64Url.DecodeFromChars(name),
                    Constants.CipherId.ENCODING_BASE4K => SecombaBase4K.Decode(name),
                    _ => throw new ArgumentOutOfRangeException(nameof(fileNameEncodingId))
                };

                // Decrypt
                var plaintextNameBuffer = DecryptFileName(decoded, associatedData);
                if (plaintextNameBuffer is null)
                    return null;

                // Get string from plaintext buffer
                return Encoding.UTF8.GetString(plaintextNameBuffer);
            }
        }

        protected abstract byte[] EncryptFileName(ReadOnlySpan<byte> plaintextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        protected abstract byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
