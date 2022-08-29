using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    /// <inheritdoc cref="IHmacSha256Crypt"/>
    internal sealed class HmacSha256Crypt : IHmacSha256Crypt
    {
        private IncrementalHash? _incrementalHash;

        public void InitializeHmac(ReadOnlySpan<byte> key)
        {
            _incrementalHash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
        }

        /// <inheritdoc/>
        public void Update(ReadOnlySpan<byte> bytes)
        {
            ArgumentNullException.ThrowIfNull(_incrementalHash);
            _incrementalHash.AppendData(bytes);
        }

        /// <inheritdoc/>
        public void DoFinal(ReadOnlySpan<byte> bytes)
        {
            ArgumentNullException.ThrowIfNull(_incrementalHash);
            _incrementalHash.AppendData(bytes);
        }

        /// <inheritdoc/>
        public void GetHash(Span<byte> destination)
        {
            ArgumentNullException.ThrowIfNull(_incrementalHash);
            _incrementalHash.GetCurrentHash(destination);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _incrementalHash?.Dispose();
            _incrementalHash = null;
        }
    }
}
