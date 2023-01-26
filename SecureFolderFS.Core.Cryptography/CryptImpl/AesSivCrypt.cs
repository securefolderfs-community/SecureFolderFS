using Miscreant;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.CryptImpl
{
    /// <inheritdoc cref="IAesSivCrypt"/>
    public sealed class AesSivCrypt : IAesSivCrypt
    {
        private Aead? _aesCmacSiv;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData)
        {
            InitializeAead(encKey, macKey);
            return _aesCmacSiv!.Seal(bytes.ToArray(), data: associatedData.ToArray());
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[]? Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData)
        {
            InitializeAead(encKey, macKey);

            try
            {
                return _aesCmacSiv!.Open(bytes.ToArray(), data: associatedData.ToArray());
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        private void InitializeAead(ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey)
        {
            if (_aesCmacSiv is null)
            {
                // The longKey will be split into two keys - one for S2V and the other one for CTR
                var longKey = new byte[encKey.Length + macKey.Length];
                longKey.EmplaceArrays(encKey.ToArray(), macKey.ToArray());

                _aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _aesCmacSiv?.Dispose();
        }
    }
}
