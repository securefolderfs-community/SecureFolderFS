using SecureFolderFS.Core.Cryptography.SecureStore;

namespace SecureFolderFS.Core.Cryptography.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="SecretKey"/> class.
    /// </summary>
    public static class SecretKeyExtensions
    {
        /// <summary>
        /// Creates a unique copy of the specified <see cref="SecretKey"/> and disposes the original key.
        /// </summary>
        /// <param name="originalKey">The original <see cref="SecretKey"/> to copy.</param>
        /// <returns>A new copy of the <see cref="SecretKey"/>.</returns>
        public static SecretKey CreateUniqueCopy(this SecretKey originalKey)
        {
            var copiedKey = originalKey.CreateCopy();
            originalKey.Dispose();

            return copiedKey;
        }
    }
}
