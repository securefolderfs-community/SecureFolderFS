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
        public ManagedKey DekKey { get; }

        /// <summary>
        /// Gets the Message Authentication Code (MAC) key.
        /// </summary>
        public ManagedKey MacKey { get; }

        private KeyPair(ManagedKey dekKey, ManagedKey macKey)
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
        public static KeyPair ImportKeys(ManagedKey dekKeyToDestroy, ManagedKey macKeyToDestroy)
        {
            return new(dekKeyToDestroy.CreateUniqueCopy(), macKeyToDestroy.CreateUniqueCopy());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Convert.ToBase64String(DekKey)}{Constants.KeyTraits.KEY_TEXT_SEPARATOR}{Convert.ToBase64String(MacKey)}";
        }

        /// <summary>
        /// Combines the provided encoded recovery key into a <see cref="ManagedKey"/> instance.
        /// </summary>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <returns>A <see cref="ManagedKey"/> instance representing the combined recovery key.</returns>
        public static ManagedKey CombineRecoveryKey(string encodedRecoveryKey)
        {
            var keySplit = encodedRecoveryKey.ReplaceLineEndings(string.Empty).Split(Constants.KeyTraits.KEY_TEXT_SEPARATOR);
            using var recoveryKey = new ManagedKey(Constants.KeyTraits.DEK_KEY_LENGTH + Constants.KeyTraits.MAC_KEY_LENGTH);

            if (!Convert.TryFromBase64String(keySplit[0], recoveryKey.Key.AsSpan(0, Constants.KeyTraits.DEK_KEY_LENGTH), out _))
                throw new FormatException("The recovery key (1) was not in the correct format.");

            if (!Convert.TryFromBase64String(keySplit[1], recoveryKey.Key.AsSpan(Constants.KeyTraits.DEK_KEY_LENGTH, Constants.KeyTraits.MAC_KEY_LENGTH), out _))
                throw new FormatException("The recovery key (2) was not in the correct format.");

            return recoveryKey.CreateCopy();
        }

        /// <summary>
        /// Creates a <see cref="KeyPair"/> from the specified recovery key.
        /// </summary>
        /// <param name="recoveryKey">The combined recovery key.</param>
        /// <returns>A <see cref="KeyPair"/> instance representing the key pair.</returns>
        public static KeyPair CopyFromRecoveryKey(ManagedKey recoveryKey)
        {
            var dekKey = new ManagedKey(Constants.KeyTraits.DEK_KEY_LENGTH);
            var macKey = new ManagedKey(Constants.KeyTraits.MAC_KEY_LENGTH);

            recoveryKey.Key.AsSpan(0, Constants.KeyTraits.DEK_KEY_LENGTH).CopyTo(dekKey.Key);
            recoveryKey.Key.AsSpan(Constants.KeyTraits.DEK_KEY_LENGTH, Constants.KeyTraits.MAC_KEY_LENGTH).CopyTo(macKey.Key);

            return new KeyPair(dekKey, macKey);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DekKey.Dispose();
            MacKey.Dispose();
        }
    }
}
