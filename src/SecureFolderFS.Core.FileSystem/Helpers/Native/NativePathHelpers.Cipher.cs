using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Native
{
    /// <summary>
    /// A set of file system path management helpers that only work in a native environment with unlimited file system access.
    /// </summary>
    public static partial class NativePathHelpers
    {
        /// <summary>
        /// Encrypts and gets the ciphertext path from provided <paramref name="plaintextRelativePath"/>.
        /// </summary>
        /// <param name="plaintextRelativePath">The relative plaintext path to an item.</param>
        /// <param name="specifics">The specifics.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <remarks>
        /// The <paramref name="expendableDirectoryId"/> parameter should be initialized with a correct size.
        /// <br/>
        /// To reduce allocations, it is reusable and its only purpose is to hold Directory ID data.
        /// </remarks>
        /// <returns>A full ciphertext path.</returns>
        public static string GetCiphertextPath(string plaintextRelativePath, FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            // Make path relative as a precaution (if the path is passed as ContentPath + PlaintextPath)
            plaintextRelativePath = MakeRelative(plaintextRelativePath, specifics.ContentFolder.Id);

            // Return the (full) path, if not using name encryption
            if (specifics.Security.NameCrypt is null)
                return Path.Combine(specifics.ContentFolder.Id, plaintextRelativePath);

            var finalPath = specifics.ContentFolder.Id;
            foreach (var namePart in plaintextRelativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Get the Directory ID
                // Important: We cannot just combine the ciphertext (final) path with DIRECTORY_ID_FILENAME since the directory may be the storage root.
                //      If the directory is in turn storage root, the Directory ID is empty, thus we must use GetDirectoryId
                var result = GetDirectoryId(Path.Combine(finalPath, namePart), specifics, expendableDirectoryId);

                // Encrypt the name. Use ReadOnlySpan<byte>.Empty only when we are in the storage root directory
                var ciphertextName = specifics.Security.NameCrypt.EncryptName(namePart, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                // Combine the final path and append extension
                finalPath = Path.Combine(finalPath, $"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}");
            }

            return finalPath;
        }

        /// <summary>
        /// Decrypts and gets the plaintext path from provided <paramref name="ciphertextPath"/>.
        /// </summary>
        /// <param name="ciphertextPath">The relative plaintext path to an item.</param>
        /// <param name="specifics">The specifics.</param>
        /// <param name="expendableDirectoryId">A <see cref="Span{T}"/> of size <see cref="Constants.DIRECTORY_ID_SIZE"/> which will be used to hold the Directory ID data.</param>
        /// <remarks>
        /// The <paramref name="expendableDirectoryId"/> parameter should be initialized with a correct size.
        /// <br/>
        /// To reduce allocations, it is reusable and its only purpose is to hold Directory ID data.
        /// </remarks>
        /// <returns>A relative plaintext path.</returns>
        public static string? GetPlaintextPath(string ciphertextPath, FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            // Use relative path in both cases
            ciphertextPath = MakeRelative(ciphertextPath, specifics.ContentFolder.Id);

            // Return the (relative) path, if not using name encryption
            if (specifics.Security.NameCrypt is null)
                return ciphertextPath;

            var finalPath = specifics.ContentFolder.Id;
            var finalCiphertextPath = specifics.ContentFolder.Id;

            foreach (var namePart in ciphertextPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Remove encrypted file extension
                var clearedNamePart = Path.GetFileNameWithoutExtension(namePart);

                // Get the Directory ID
                // Important: The ciphertext path must be used with a file name to retrieve the Directory ID.
                //      In addition, we cannot just combine the ciphertext path with DIRECTORY_ID_FILENAME since the directory may be the storage root.
                //      If the directory is in turn storage root, the Directory ID is empty, thus we must use GetDirectoryId
                var result = GetDirectoryId(Path.Combine(finalCiphertextPath, namePart), specifics, expendableDirectoryId);

                // Decrypt the name. Use ReadOnlySpan<byte>.Empty only when we are in the storage root directory
                var plaintextName = specifics.Security.NameCrypt.DecryptName(clearedNamePart, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
                if (plaintextName is null)
                    return null;

                // Combine the final path
                finalPath = Path.Combine(finalPath, plaintextName);

                // Combine the final ciphertext path with ciphertext name (that has the encrypted file extension)
                finalCiphertextPath = Path.Combine(finalCiphertextPath, namePart);
            }

            return finalPath;
        }
    }
}
