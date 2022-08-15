using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class HmacSha256Crypt : IHmacSha256Crypt
    {
        private readonly bool _isBaseInstance;

        private IncrementalHash _incrementalHash;

        private HmacSha256Crypt(bool baseInstance)
        {
            _isBaseInstance = baseInstance;
        }

        internal static HmacSha256Crypt GetBaseInstance()
        {
            return new HmacSha256Crypt(true);
        }

        public IHmacSha256Crypt GetInstance()
        {
            AssertIsBaseInstance();

            return new HmacSha256Crypt(false);
        }

        public void InitializeHMAC(byte[] key)
        {
            AssertNotBaseInstance();

            _incrementalHash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
        }

        public void Update(ReadOnlySpan<byte> bytes)
        {
            AssertNotBaseInstance();

            _incrementalHash.AppendData(bytes);
        }

        public void DoFinal(ReadOnlySpan<byte> bytes)
        {
            AssertNotBaseInstance();

            _incrementalHash.AppendData(bytes);
        }

        public byte[] GetHash()
        {
            AssertNotBaseInstance();

            return _incrementalHash.GetCurrentHash();
        }

        public void GetHash(Span<byte> destination)
        {
            AssertNotBaseInstance();

            _incrementalHash.GetCurrentHash(destination);
        }

        private void AssertNotBaseInstance()
        {
            if (_isBaseInstance)
            {
                throw new InvalidOperationException("Cannot create HMAC using the base instance, a new instance must be created first.");
            }
        }

        private void AssertIsBaseInstance()
        {
            if (!_isBaseInstance)
            {
                throw new InvalidOperationException("Cannot perform this operation on non-base instance object.");
            }
        }

        public void Dispose()
        {
            _incrementalHash?.Dispose();
        }
    }
}
