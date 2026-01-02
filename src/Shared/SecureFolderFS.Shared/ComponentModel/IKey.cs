using System;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a key material that can be disposed of.
    /// </summary>
    public interface IKey : ILengthMeasurable, IDisposable
    {
    }

    /// <summary>
    /// Represents a disposable key material that can expose its byte sequence.
    /// </summary>
    public interface IKeyBytes : IKeyUsage
    {
        /// <summary>
        /// Gets the byte sequence representing the key.
        /// </summary>
        byte[] Key { get; }
    }

    /// <summary>
    /// Represents a key as a disposable byte sequence with actions to use it as a readonly span of bytes.
    /// </summary>
    public interface IKeyUsage : IKey
    {
        /// <summary>
        /// Executes the provided action with the key represented as a <see cref="ReadOnlySpan{T}"/> of bytes.
        /// </summary>
        /// <param name="keyAction">An action to execute, receiving the key as a <see cref="ReadOnlySpan{T}"/> of bytes.</param>
        void UseKey(Action<ReadOnlySpan<byte>> keyAction);

        /// <summary>
        /// Executes the provided action with the key represented as a <see cref="ReadOnlySpan{T}"/> of bytes.
        /// </summary>
        /// <param name="keyAction">An action to execute, receiving the key as a <see cref="ReadOnlySpan{T}"/> of bytes.</param>
        /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
        /// <returns>The result of the executed action, of type <typeparamref name="TResult"/>.</returns>
        TResult UseKey<TResult>(Func<ReadOnlySpan<byte>, TResult> keyAction);
    }
}
