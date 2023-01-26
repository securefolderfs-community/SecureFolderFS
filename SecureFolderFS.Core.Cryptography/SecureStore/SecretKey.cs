using System;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <summary>
    /// Represents a secret key store.
    /// </summary>
    public abstract class SecretKey : IDisposable
    {
        /// <summary>
        /// Gets the underlying byte representation of the key.
        /// </summary>
        public abstract byte[] Key { get; }

        /// <summary>
        /// Creates a standalone copy of the key.
        /// </summary>
        /// <returns>A new copy of <see cref="SecretKey"/>.</returns>
        public abstract SecretKey CreateCopy();

        /// <summary>
        /// Converts <paramref name="secretKey"/> into <see cref="ReadOnlySpan{T}"/> of type <see cref="byte"/>.
        /// </summary>
        /// <param name="secretKey">The <see cref="SecretKey"/> instance to convert.</param>
        public static implicit operator ReadOnlySpan<byte>(SecretKey secretKey) => secretKey.Key;

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
