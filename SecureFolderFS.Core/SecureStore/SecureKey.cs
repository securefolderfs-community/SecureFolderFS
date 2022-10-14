using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.SecureStore
{
    /// <inheritdoc cref="SecretKey"/>
    internal sealed class SecureKey : SecretKey
    {
        /// <inheritdoc/>
        public override byte[] Key { get; }

        public SecureKey(byte[] key)
        {
            Key = key;
        }

        /// <inheritdoc/>
        public override SecretKey CreateCopy()
        {
            var keyCopy = new byte[Key.Length];
            Array.Copy(Key, 0, keyCopy, 0, Key.Length);

            return new SecureKey(keyCopy);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Array.Clear(Key);
        }
    }
}
