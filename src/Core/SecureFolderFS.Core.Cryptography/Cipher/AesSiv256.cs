using System;
using System.Runtime.CompilerServices;
using Miscreant;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public sealed class AesSiv256 : IDisposable
    {
        private readonly Aead _aesCmacSiv;

        private AesSiv256(Aead aesCmacSiv)
        {
            _aesCmacSiv = aesCmacSiv;
        }

        public static AesSiv256 CreateInstance(ReadOnlySpan<byte> dekKey, ReadOnlySpan<byte> macKey)
        {
            // The longKey will be split into two keys - one for S2V and the other one for CTR
            var longKey = new byte[dekKey.Length + macKey.Length];
            var longKeySpan = longKey.AsSpan();

            // Copy keys
            dekKey.CopyTo(longKeySpan);
            macKey.CopyTo(longKeySpan.Slice(dekKey.Length));

            var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return new AesSiv256(aesCmacSiv);
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

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                _aesCmacSiv.Dispose();
            }
            catch (Exception ex)
            {
                // TODO: Investigate. Sometimes an exception is thrown when disposing the Aead instance
                _ = ex;
            }
        }
    }
}
