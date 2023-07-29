using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.SecureStore
{
    /// <inheritdoc cref="SecretKey"/>
    internal sealed class SecureKey : SecretKey
    {
        /// <inheritdoc/>
        public override byte[] Key { get; }

        public SecureKey(int size)
        {
            Key = new byte[size];
        }

        /// <inheritdoc/>
        public override SecretKey CreateCopy()
        {
            var secureKey = new SecureKey(Key.Length);
            Array.Copy(Key, 0, secureKey.Key, 0, Key.Length);

            return secureKey;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Array.Clear(Key);
        }
    }
}
