using System;
using System.Buffers;
using System.Security.Cryptography;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <inheritdoc cref="IKeyBytes"/>
    public sealed class ManagedKey : IKeyBytes, ICloneable
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
        public void UseKey<TState>(TState state, ReadOnlySpanAction<byte, TState> keyAction)
        {
            keyAction(Key, state);
        }

        /// <inheritdoc/>
        public TResult UseKey<TResult>(Func<ReadOnlySpan<byte>, TResult> keyAction)
        {
            return keyAction(Key);
        }

        /// <inheritdoc/>
        public TResult UseKey<TState, TResult>(TState state, ReadOnlySpanFunc<byte, TState, TResult> keyAction)
        {
            return keyAction(Key, state);
        }

        /// <inheritdoc/>
        public object Clone()
        {
            var secureKey = new ManagedKey(Key.Length);
            Array.Copy(Key, 0, secureKey.Key, 0, Key.Length);

            return secureKey;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(Key);
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