﻿using Miscreant;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    // TODO: Needs docs
    public sealed class AesSiv128 : IDisposable
    {
        private readonly Aead _aesCmacSiv;

        private AesSiv128(Aead aesCmacSiv)
        {
            _aesCmacSiv = aesCmacSiv;
        }

        public static AesSiv128 CreateInstance(ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey)
        {
            // The longKey will be split into two keys - one for S2V and the other one for CTR
            var longKey = new byte[encKey.Length + macKey.Length];
            var longKeySpan = longKey.AsSpan();

            // Copy keys
            encKey.CopyTo(longKeySpan);
            macKey.CopyTo(longKeySpan.Slice(encKey.Length));

            var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);

            return new AesSiv128(aesCmacSiv);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            return _aesCmacSiv.Seal(bytes.ToArray(), data: associatedData.ToArray());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            return _aesCmacSiv.Open(bytes.ToArray(), data: associatedData.ToArray());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[]? TryDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            try
            {
                return Decrypt(bytes, associatedData);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _aesCmacSiv.Dispose();
        }
    }
}
