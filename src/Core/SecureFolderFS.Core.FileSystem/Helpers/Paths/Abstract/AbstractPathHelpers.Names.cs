using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.FileNames;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name.</returns>
        public static async Task<string> EncryptNameAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, directoryId, cancellationToken);

            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? directoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The name to decrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a decrypted name.</returns>
        public static async Task<string?> DecryptNameAsync(string ciphertextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            try
            {
                if (specifics.Security.NameCrypt is null)
                    return ciphertextName;

                var directoryId = AllocateDirectoryId(specifics.Security, ciphertextName);
                var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, directoryId, cancellationToken);

                var normalizedName = NoCipherExtensionNormalizedName(ciphertextName);
                return specifics.Security.NameCrypt.DecryptName(normalizedName, result ? directoryId : ReadOnlySpan<byte>.Empty);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/> by utilizing a plaintext name cache, or directly if the cache is unavailable.
        /// </summary>
        /// <param name="ciphertextName">The encrypted name to decrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the decrypted name, retrieved from the cache if available, or null if it is not found and decryption fails.</returns>
        public static async Task<string?> CacheDecryptNameAsync(string ciphertextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            if (!specifics.PlaintextFileNameCache.IsAvailable)
                return await DecryptNameAsync(ciphertextName, ciphertextParentFolder, specifics, cancellationToken);

            var directoryId = AllocateDirectoryId(specifics.Security, ciphertextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, directoryId, cancellationToken);

            var cacheKey = new NameWithDirectoryId(result ? directoryId : [], ciphertextName);
            var cachedName = specifics.PlaintextFileNameCache.CacheGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedName))
                return cachedName;

            var decryptedName = await DecryptNameAsync(ciphertextName, ciphertextParentFolder, specifics, cancellationToken);
            if (string.IsNullOrEmpty(decryptedName))
                return null;

            specifics.PlaintextFileNameCache.CacheSet(cacheKey, decryptedName);
            return decryptedName;
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/> by utilizing a ciphertext name cache, or directly if the cache is unavailable.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is an encrypted name, retrieved from the cache if available, or newly encrypted if not.</returns>
        public static async Task<string> CacheEncryptNameAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            if (!specifics.CiphertextFileNameCache.IsAvailable)
                return await EncryptNameAsync(plaintextName, ciphertextParentFolder, specifics, cancellationToken);

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, directoryId, cancellationToken);

            var cacheKey = new NameWithDirectoryId(result ? directoryId : [], plaintextName);
            var cachedName = specifics.CiphertextFileNameCache.CacheGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedName))
                return cachedName;

            var encryptedName = await EncryptNameAsync(plaintextName, ciphertextParentFolder, specifics, cancellationToken);
            specifics.CiphertextFileNameCache.CacheSet(cacheKey, encryptedName);
            return encryptedName;
        }
    }
}
