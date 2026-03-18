using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class DisposablePassword : IPassword
    {
        /// <inheritdoc/>
        public byte[] Key { get; }

        /// <inheritdoc/>
        public int CharacterCount { get; }

        /// <inheritdoc/>
        public int Length { get; }

        public DisposablePassword(string password)
        {
            Key = Encoding.UTF8.GetBytes(password);
            Length = Key.Length;
            CharacterCount = password.Length;
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

        /// <inheritdoc cref="IPassword.ToString"/>
        public new string ToString()
        {
            return Encoding.UTF8.GetString(Key);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(Key);
        }
    }
}
