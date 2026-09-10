using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Miscreant;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public sealed class AesSiv256 : IDisposable
    {
        private readonly Aead? _aesCmacSiv;
        private readonly bool _preferBouncyCastle;

        /// <summary>
        /// Holds the concatenated DEK and MAC key.
        /// </summary>
        /// <remarks>
        /// Allocated pinned so the garbage collector cannot relocate it and leave copies of the
        /// master keys scattered across the heap, and zeroed in <see cref="Dispose"/> so it does
        /// not survive in a memory image after the vault is locked.
        /// </remarks>
        private readonly byte[] _longKey;

        private AesSiv256(Aead? aesCmacSiv, byte[] longKey, bool preferBouncyCastle)
        {
            _aesCmacSiv = aesCmacSiv;
            _longKey = longKey;
            _preferBouncyCastle = preferBouncyCastle;
        }

        public static AesSiv256 CreateInstance(ReadOnlySpan<byte> dekKey, ReadOnlySpan<byte> macKey)
        {
            // The longKey will be split into two keys - one for S2V and the other one for CTR
            var longKey = GC.AllocateArray<byte>(dekKey.Length + macKey.Length, pinned: true);
            try
            {
                var longKeySpan = longKey.AsSpan();

                // Copy keys
                dekKey.CopyTo(longKeySpan);
                macKey.CopyTo(longKeySpan.Slice(dekKey.Length));

                if (Constants.PreferBouncyCastle)
                    return new AesSiv256(null, longKey, true);

                var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
                return new AesSiv256(aesCmacSiv, longKey, false);
            }
            catch (Exception)
            {
                CryptographicOperations.ZeroMemory(longKey);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            if (_preferBouncyCastle)
                return BouncyCastleAesSiv.Seal(_longKey, associatedData, bytes);

            return _aesCmacSiv!.Seal(bytes.ToArray(), data: associatedData.ToArray());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> associatedData)
        {
            if (_preferBouncyCastle)
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
            finally
            {
                // Zero the master key material last, so the AEAD is torn down before its key disappears
                CryptographicOperations.ZeroMemory(_longKey);
            }
        }
    }
}
