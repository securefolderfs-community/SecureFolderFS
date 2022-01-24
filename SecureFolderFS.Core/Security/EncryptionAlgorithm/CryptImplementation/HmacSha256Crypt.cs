using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class HmacSha256Crypt : HMACSHA256, IHmacSha256Crypt
    {
        private readonly bool _isBaseInstance;

        private HmacSha256Crypt()
        {
            this._isBaseInstance = true;
        }

        private HmacSha256Crypt(byte[] key)
            : base(key)
        {
        }

        internal static HmacSha256Crypt GetBaseInstance()
        {
            return new HmacSha256Crypt();
        }

        public IHmacSha256Crypt GetInstance(byte[] key)
        {
            AssertIsBaseInstance();

            return new HmacSha256Crypt(key);
        }

        public void InitializeHMAC()
        {
            AssertNotBaseInstance();

            //base.Initialize();
        }

        public void Update(byte[] bytes)
        {
            AssertNotBaseInstance();

            base.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        public void DoFinal(byte[] bytes)
        {
            AssertNotBaseInstance();

            base.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public byte[] GetHash()
        {
            AssertNotBaseInstance();

            return Hash;
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
    }
}
