using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher.Default
{
    /// <inheritdoc cref="IHmacSha256Crypt"/>
    public sealed class HmacSha256Crypt : IHmacSha256Crypt
    {
        public int MacSize { get; } = 256 / 8;

        /// <inheritdoc/>
        public IHmacSha256Instance GetInstance()
        {
            return new HmacSha256Instance();
        }

        private sealed class HmacSha256Instance : IHmacSha256Instance
        {
            private IncrementalHash? _incrementalHash;

            /// <inheritdoc/>
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
            public int GetHash(Span<byte> destination)
            {
                ArgumentNullException.ThrowIfNull(_incrementalHash);
                return _incrementalHash.GetCurrentHash(destination);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _incrementalHash?.Dispose();
                _incrementalHash = null;
            }
        }
    }
}
