using SecureFolderFS.Core.Cryptography.SecureStore;

namespace SecureFolderFS.Core.Cryptography.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="ManagedKey"/> class.
    /// </summary>
    public static class ManagedKeyExtensions
    {
        /// <summary>
        /// Creates a unique copy of the specified <see cref="ManagedKey"/> and disposes the original key.
        /// </summary>
        /// <param name="originalKey">The original <see cref="ManagedKey"/> to copy.</param>
        /// <returns>A new copy of the <see cref="ManagedKey"/>.</returns>
        public static ManagedKey CreateUniqueCopy(this ManagedKey originalKey)
        {
            var copiedKey = originalKey.CreateCopy();
            originalKey.Dispose();

            return copiedKey;
        }
    }
}
