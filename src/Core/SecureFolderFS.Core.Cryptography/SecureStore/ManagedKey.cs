using System;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <inheritdoc cref="IKeyBytes"/>
    public sealed class ManagedKey : IKeyBytes
    {
        /// <inheritdoc/>
        public byte[] Key { get; }

        /// <inheritdoc/>
        public int Length { get; }

        public ManagedKey(int size)
        {
            Key = new byte[size];
            Length = size;
        }

        private ManagedKey(byte[] key)
        {
            Key = key;
            Length = key.Length;
        }

        /// <inheritdoc/>
        public void UseKey(Action<ReadOnlySpan<byte>> keyAction)
        {
            keyAction(Key);
        }

        /// <inheritdoc/>
        public TResult UseKey<TResult>(Func<ReadOnlySpan<byte>, TResult> keyAction)
        {
            return keyAction(Key);
        }

        /// <summary>
        /// Creates a standalone copy of the key.
        /// </summary>
        /// <returns>A new copy of <see cref="ManagedKey"/>.</returns>
        public ManagedKey CreateCopy()
        {
            var secureKey = new ManagedKey(Key.Length);
            Array.Copy(Key, 0, secureKey.Key, 0, Key.Length);

            return secureKey;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Array.Clear(Key);
        }

        /// <summary>
        /// Takes the ownership of the provided key and manages its lifetime.
        /// </summary>
        /// <param name="key">The key to import.</param>
        public static ManagedKey TakeOwnership(byte[] key)
        {
            return new ManagedKey(key);
        }

        /// <summary>
        /// Converts <paramref name="managedKey"/> into <see cref="ReadOnlySpan{T}"/> of type <see cref="byte"/>.
        /// </summary>
        /// <param name="managedKey">The <see cref="ManagedKey"/> instance to convert.</param>
        public static implicit operator ReadOnlySpan<byte>(ManagedKey managedKey) => managedKey.Key;
    }
}