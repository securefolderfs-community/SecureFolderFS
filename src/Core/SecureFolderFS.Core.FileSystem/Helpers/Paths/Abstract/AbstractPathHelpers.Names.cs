using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.FileNames;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        #region Encrypt Name Non-Materialized

        /// <inheritdoc cref="EncryptNameAsync(string,OwlCore.Storage.IFolder,SecureFolderFS.Core.FileSystem.FileSystemSpecifics,System.Byte[],System.Threading.CancellationToken)"/>
        public static async Task<string> EncryptNameAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            return await EncryptNameAsync(plaintextName, ciphertextParentFolder, specifics, directoryId, cancellationToken);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A buffer of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended.</returns>
        public static async Task<string> EncryptNameAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, byte[]? expendableDirectoryId = null, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            expendableDirectoryId ??= AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, expendableDirectoryId, cancellationToken);

            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="contentFolder">The content folder.</param>
        /// <param name="security">The <see cref="Security"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended.</returns>
        public static async Task<string> EncryptNameAsync(string plaintextName, IFolder ciphertextParentFolder, IFolder contentFolder,
            Security security, CancellationToken cancellationToken = default)
        {
            if (security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, contentFolder, directoryId, cancellationToken);

            return security.NameCrypt.EncryptName(plaintextName, result ? directoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        #endregion

        #region Encrypt Name Materialized

        /// <inheritdoc cref="EncryptNameForUseAsync(string,OwlCore.Storage.IFolder,SecureFolderFS.Core.FileSystem.FileSystemSpecifics,System.Byte[],System.Threading.CancellationToken)"/>
        public static async Task<string> EncryptNameForUseAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            return await EncryptNameForUseAsync(plaintextName, ciphertextParentFolder, specifics, directoryId, cancellationToken);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/> and materializes it.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A buffer of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended.</returns>
        public static async Task<string> EncryptNameForUseAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, byte[]? expendableDirectoryId = null, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            expendableDirectoryId ??= AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, expendableDirectoryId, cancellationToken);

            var encryptedName = specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
            if (specifics.Options.ShorteningThreshold > 0 && encryptedName.Length >= specifics.Options.ShorteningThreshold)
            {
                var shortenedBase = ComputeShortenedNameBase(encryptedName);
                await WriteSidecarAsync(ciphertextParentFolder, shortenedBase, encryptedName, cancellationToken);
                return shortenedBase + Constants.Names.SHORTENED_FILE_EXTENSION;
            }

            return encryptedName;
        }

        #endregion

        #region Encrypt Name Discoverability

        /// <inheritdoc cref="EncryptNameForDiscoveryAsync(string,OwlCore.Storage.IFolder,SecureFolderFS.Core.FileSystem.FileSystemSpecifics,System.Byte[],System.Threading.CancellationToken)"/>
        public static async Task<string> EncryptNameForDiscoveryAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            return await EncryptNameForDiscoveryAsync(plaintextName, ciphertextParentFolder, specifics, directoryId, cancellationToken);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/> and discovers potential shortening branch.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A buffer of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended.</returns>
        public static async Task<string> EncryptNameForDiscoveryAsync(string plaintextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, byte[]? expendableDirectoryId = null, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            expendableDirectoryId ??= AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, expendableDirectoryId, cancellationToken);

            var encryptedName = specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
            if (specifics.Options.ShorteningThreshold > 0 && encryptedName.Length >= specifics.Options.ShorteningThreshold)
            {
                var shortenedBase = ComputeShortenedNameBase(encryptedName);
                return shortenedBase + Constants.Names.SHORTENED_FILE_EXTENSION;
            }

            return encryptedName;
        }

        #endregion

        /// <summary>
        /// Encrypts a plaintext name using the specified Directory ID and security parameters.
        /// </summary>
        /// <param name="plaintextName">The original plaintext name to encrypt.</param>
        /// <param name="newDirectoryId">A new Directory ID used for encryption.</param>
        /// <param name="security">The <see cref="Security"/> instance associated with the item.</param>
        /// <returns>The encrypted name with the appropriate file extension appended.</returns>
        public static string EncryptNewName(string plaintextName, byte[] newDirectoryId, Security security)
        {
            if (security.NameCrypt is null)
                return plaintextName;

            return security.NameCrypt.EncryptName(plaintextName, newDirectoryId) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        /// <summary>
        /// Encrypts a plaintext name using the specified Directory ID and materializes it, writing a sidecar file if shortening applies.
        /// </summary>
        /// <param name="plaintextName">The original plaintext name to encrypt.</param>
        /// <param name="newDirectoryId">A new Directory ID used for encryption.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder where the sidecar will be written if needed.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended.</returns>
        public static async Task<string> EncryptNewNameForUseAsync(string plaintextName, byte[] newDirectoryId, IFolder ciphertextParentFolder, FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var encryptedName = specifics.Security.NameCrypt.EncryptName(plaintextName, newDirectoryId) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
            if (specifics.Options.ShorteningThreshold > 0 && encryptedName.Length >= specifics.Options.ShorteningThreshold)
            {
                var shortenedBase = ComputeShortenedNameBase(encryptedName);
                await WriteSidecarAsync(ciphertextParentFolder, shortenedBase, encryptedName, cancellationToken);
                return shortenedBase + Constants.Names.SHORTENED_FILE_EXTENSION;
            }

            return encryptedName;
        }

        /// <inheritdoc cref="DecryptNameAsync(string,OwlCore.Storage.IFolder,SecureFolderFS.Core.FileSystem.FileSystemSpecifics,System.Byte[],System.Threading.CancellationToken)"/>
        public static async Task<string?> DecryptNameAsync(string ciphertextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            var directoryId = AllocateDirectoryId(specifics.Security, ciphertextName);
            return await DecryptNameAsync(ciphertextName, ciphertextParentFolder, specifics, directoryId, cancellationToken);
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The name to decrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A buffer of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a decrypted name.</returns>
        public static async Task<string?> DecryptNameAsync(string ciphertextName, IFolder ciphertextParentFolder,
            FileSystemSpecifics specifics, byte[]? expendableDirectoryId, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            // Sidecar files are internal bookkeeping - they have no plaintext name
            if (IsSidecarName(ciphertextName))
                return null;

            try
            {
                // Resolve shortened names to their full ciphertext name via the paired sidecar
                if (ciphertextName.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    var shortenedBase = RemoveShortenedExtension(ciphertextName).ToString();
                    var resolvedName = await ReadSidecarAsync(ciphertextParentFolder, shortenedBase, cancellationToken);
                    if (resolvedName is null)
                        return null;

                    ciphertextName = resolvedName;
                }

                expendableDirectoryId ??= AllocateDirectoryId(specifics.Security, ciphertextName);
                var result = await GetDirectoryIdAsync(ciphertextParentFolder, specifics, expendableDirectoryId, cancellationToken);

                var normalizedName = RemoveCiphertextExtension(ciphertextName);
                return specifics.Security.NameCrypt.DecryptName(normalizedName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
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
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name with the appropriate file extension appended, retrieved from the cache if available, or newly encrypted if not.</returns>
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
