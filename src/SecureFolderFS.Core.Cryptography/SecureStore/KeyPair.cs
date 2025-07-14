using SecureFolderFS.Core.Cryptography.Extensions;
using System;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <summary>
    /// Represents a chain of secret keys used for encryption and message authentication.
    /// </summary>
    public sealed class KeyPair : IDisposable
    {
        /// <summary>
        /// Gets the Data Encryption Key (DEK).
        /// </summary>
        public SecretKey DekKey { get; }

        /// <summary>
        /// Gets the Message Authentication Code (MAC) key.
        /// </summary>
        public SecretKey MacKey { get; }

        private KeyPair(SecretKey dekKey, SecretKey macKey)
        {
            DekKey = dekKey;
            MacKey = macKey;
        }

        /// <summary>
        /// Imports the specified DEK and MAC keys, creating unique copies of them and disposing the original instances.
        /// </summary>
        /// <param name="dekKeyToDestroy">The DEK to import.</param>
        /// <param name="macKeyToDestroy">The MAC key to import.</param>
        /// <remarks>
        /// This method copies the imported keys and disposed of the original instances.
        /// Make sure no other classes access the passed keys after they are imported.
        /// Instead, use <see cref="DekKey"/> and <see cref="MacKey"/> instances.
        /// </remarks>
        /// <returns>A new instance of the <see cref="KeyPair"/> class with the imported keys.</returns>
        public static KeyPair ImportKeys(SecretKey dekKeyToDestroy, SecretKey macKeyToDestroy)
        {
            return new(dekKeyToDestroy.CreateUniqueCopy(), macKeyToDestroy.CreateUniqueCopy());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Convert.ToBase64String(DekKey)}{Constants.KeyTraits.KEY_TEXT_SEPARATOR}{Convert.ToBase64String(MacKey)}";
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DekKey.Dispose();
            MacKey.Dispose();
        }
    }
}
