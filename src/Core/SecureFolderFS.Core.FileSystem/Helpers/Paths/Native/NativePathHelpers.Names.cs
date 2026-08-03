using System;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    public static partial class NativePathHelpers
    {
        #region Encrypt Name Non-Materialized

        /// <inheritdoc cref="EncryptName(string,string,FileSystemSpecifics,Span{byte})"/>
        public static string EncryptName(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, plaintextName);
            return EncryptName(plaintextName, ciphertextParentFolder, specifics, directoryId);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder path.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <returns>An encrypted name with the appropriate file extension appended.</returns>
        public static string EncryptName(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var result = GetDirectoryId(ciphertextParentFolder, specifics, expendableDirectoryId);
            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        #endregion

        #region Encrypt Name Materialized

        /// <inheritdoc cref="EncryptNameForUse(string,string,FileSystemSpecifics,Span{byte})"/>
        public static string EncryptNameForUse(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, plaintextName);
            return EncryptNameForUse(plaintextName, ciphertextParentFolder, specifics, directoryId);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/> and materializes it.
        /// If the encrypted name exceeds the shortening threshold, a sidecar file is written and the shortened name is returned.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder path.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <returns>An encrypted name with the appropriate file extension appended.</returns>
        public static string EncryptNameForUse(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var result = GetDirectoryId(ciphertextParentFolder, specifics, expendableDirectoryId);
            var encryptedName = specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;

            if (specifics.Options.ShorteningThreshold > 0 && encryptedName.Length >= specifics.Options.ShorteningThreshold)
            {
                var shortenedBase = AbstractPathHelpers.ComputeShortenedNameBase(encryptedName);
                WriteSidecar(ciphertextParentFolder, shortenedBase, encryptedName);
                return shortenedBase + Constants.Names.SHORTENED_FILE_EXTENSION;
            }

            return encryptedName;
        }

        #endregion

        #region Encrypt Name Discoverability

        /// <inheritdoc cref="EncryptNameForDiscovery(string,string,FileSystemSpecifics,Span{byte})"/>
        public static string EncryptNameForDiscovery(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, plaintextName);
            return EncryptNameForDiscovery(plaintextName, ciphertextParentFolder, specifics, directoryId);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/> and discovers potential shortening branch.
        /// Unlike <see cref="EncryptNameForUse(string,string,FileSystemSpecifics,Span{byte})"/>, this method does not write a sidecar file.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder path.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <returns>An encrypted name with the appropriate file extension appended.</returns>
        public static string EncryptNameForDiscovery(string plaintextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var result = GetDirectoryId(ciphertextParentFolder, specifics, expendableDirectoryId);
            var encryptedName = specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;

            if (specifics.Options.ShorteningThreshold > 0 && encryptedName.Length >= specifics.Options.ShorteningThreshold)
            {
                var shortenedBase = AbstractPathHelpers.ComputeShortenedNameBase(encryptedName);
                return shortenedBase + Constants.Names.SHORTENED_FILE_EXTENSION;
            }

            return encryptedName;
        }

        #endregion

        #region Decrypt Name

        /// <inheritdoc cref="DecryptName(string,string,FileSystemSpecifics,Span{byte})"/>
        public static string? DecryptName(string ciphertextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            // Sidecar files are internal bookkeeping - they have no plaintext name
            if (AbstractPathHelpers.IsSidecarName(ciphertextName))
                return null;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextName);
            return DecryptName(ciphertextName, ciphertextParentFolder, specifics, directoryId);
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/>.
        /// Resolves shortened names (<c>.sffsn</c>) to their full ciphertext name via the paired sidecar before decrypting.
        /// </summary>
        /// <param name="ciphertextName">The name to decrypt.</param>
        /// <param name="ciphertextParentFolder">The ciphertext parent folder path.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <returns>A decrypted name, or <see langword="null"/> if decryption fails.</returns>
        public static string? DecryptName(string ciphertextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            // Sidecar files are internal bookkeeping - they have no plaintext name
            if (AbstractPathHelpers.IsSidecarName(ciphertextName))
                return null;

            try
            {
                // Resolve shortened names to their full ciphertext name via the paired sidecar
                if (ciphertextName.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    var shortenedBase = AbstractPathHelpers.RemoveShortenedExtension(ciphertextName).ToString();
                    var resolvedName = ReadSidecar(ciphertextParentFolder, shortenedBase);
                    if (resolvedName is null)
                        return null;

                    ciphertextName = resolvedName;
                }

                var result = GetDirectoryId(ciphertextParentFolder, specifics, expendableDirectoryId);
                var normalizedName = AbstractPathHelpers.RemoveCiphertextExtension(ciphertextName);

                return specifics.Security.NameCrypt.DecryptName(normalizedName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
