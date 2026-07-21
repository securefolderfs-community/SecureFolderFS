using System;
using System.Runtime.CompilerServices;
using Miscreant;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public sealed class AesSiv256 : IDisposable
    {
        private readonly Aead? _aesCmacSiv;
        private readonly byte[]? _longKey;

        private AesSiv256(Aead? aesCmacSiv, byte[]? longKey)
        {
            _aesCmacSiv = aesCmacSiv;
            _longKey = longKey;
        }

        public static AesSiv256 CreateInstance(ReadOnlySpan<byte> dekKey, ReadOnlySpan<byte> macKey)
        {
            // The longKey will be split into two keys - one for S2V and the other one for CTR
            var longKey = new byte[dekKey.Length + macKey.Length];
            var longKeySpan = longKey.AsSpan();

            // Copy keys
            dekKey.CopyTo(longKeySpan);
            macKey.CopyTo(longKeySpan.Slice(dekKey.Length));

            if (Constants.PreferBouncyCastle)
                return new AesSiv256(null, longKey);

            var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return new AesSiv256(aesCmacSiv, null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            if (_longKey is not null)
                return BouncyCastleAesSiv.Seal(_longKey, associatedData, bytes);

            return _aesCmacSiv!.Seal(bytes.ToArray(), data: associatedData.ToArray());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            if (_longKey is not null)
                return BouncyCastleAesSiv.Open(_longKey, associatedData, bytes);

            return _aesCmacSiv!.Open(bytes.ToArray(), data: associatedData.ToArray());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                _aesCmacSiv?.Dispose();
            }
            catch (Exception ex)
            {
                // TODO: Investigate. Sometimes an exception is thrown when disposing the Aead instance
                _ = ex;
            }
        }
    }
}
