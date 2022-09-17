using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal abstract class BaseNameCrypt : INameCrypt
    {
        protected readonly SecretKey macKey;
        protected readonly SecretKey encryptionKey;
        protected readonly ICipherProvider cipherProvider;

        protected BaseNameCrypt(SecretKey macKey, SecretKey encryptionKey, ICipherProvider cipherProvider)
        {
            this.macKey = macKey;
            this.encryptionKey = encryptionKey;
            this.cipherProvider = cipherProvider;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual string EncryptName(string cleartextName, ReadOnlySpan<byte> directoryId)
        {
            var cleartextNameBuffer = Encoding.UTF8.GetBytes(cleartextName);
            var ciphertextNameBuffer = EncryptFileName(cleartextNameBuffer, directoryId);

            return EncodingHelpers.WithBase64UrlEncoding(Convert.ToBase64String(ciphertextNameBuffer));
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual string? DecryptName(string ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            var ciphertextNameBuffer = Convert.FromBase64String(EncodingHelpers.WithoutBase64UrlEncoding(ciphertextName));
            var cleartextNameBuffer = DecryptFileName(ciphertextNameBuffer, directoryId);

            return Encoding.UTF8.GetString(cleartextNameBuffer);
        }

        protected abstract void EncryptFileName(ReadOnlySpan<byte> cleartextFileNameBuffer, ReadOnlySpan<byte> directoryId, Span<byte> result);

        protected abstract bool DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId, Span<byte> result);
    }
}
