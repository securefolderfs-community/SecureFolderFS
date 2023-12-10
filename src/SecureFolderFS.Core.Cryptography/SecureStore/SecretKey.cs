using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <summary>
    /// Represents a secret key store.
    /// </summary>
    public abstract class SecretKey : IKey
    {
        /// <summary>
        /// Gets the underlying byte representation of the key.
        /// </summary>
        public abstract byte[] Key { get; }

        /// <summary>
        /// Gets the number of bytes in the <see cref="Key"/>.
        /// </summary>
        public virtual int Length => Key.Length;

        /// <inheritdoc/>
        public virtual IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)Key).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
