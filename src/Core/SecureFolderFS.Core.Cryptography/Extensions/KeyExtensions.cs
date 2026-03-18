using System;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="ManagedKey"/> class.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        /// Creates a copy of the specified <see cref="IKeyBytes"/> instance if it is cloneable.
        /// </summary>
        /// <param name="originalKey">The original <see cref="IKeyBytes"/> to copy.</param>
        /// <returns>A new copy of the <see cref="IKeyBytes"/>.</returns>
        public static TKey CreateCopy<TKey>(this TKey originalKey)
            where TKey : IKeyUsage
        {
            if (originalKey is ICloneable cloneableKey)
                return (TKey)cloneableKey.Clone();

            throw new NotSupportedException("The provided key instance is not cloneable.");
        }

        /// <summary>
        /// Creates a unique copy of the specified <typeparamref name="TKey"/> and disposes the original key.
        /// </summary>
        /// <param name="originalKey">The original <typeparamref name="TKey"/> to copy.</param>
        /// <returns>A new copy of the key.</returns>
        public static TKey CreateUniqueCopy<TKey>(this TKey originalKey)
            where TKey : IKeyUsage
        {
            var copiedKey = originalKey.CreateCopy();
            originalKey.Dispose();

            return copiedKey;
        }
    }
}
