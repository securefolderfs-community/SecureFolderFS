using System;
using System.Linq;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class SecretKey : FreeableStore<SecretKey>
    {
        public byte[] Key { get; }

        public SecretKey(byte[] key)
        {
            this.Key = key;
        }

        public override SecretKey CreateCopy()
        {
            return new SecretKey(Key.CloneArray());
        }

        public override bool Equals(SecretKey other)
        {
            if (other?.Key == null || Key == null)
            {
                return false;
            }

            return Key.SequenceEqual(other.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        protected override void SecureFree()
        {
            DisposableArray.EnsureSecureDisposal(Key);
        }

        public static implicit operator byte[](SecretKey secretKey) => secretKey.Key;
    }
}
