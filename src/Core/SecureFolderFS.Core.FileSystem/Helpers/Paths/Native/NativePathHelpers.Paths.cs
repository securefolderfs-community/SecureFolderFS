using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    public static partial class NativePathHelpers
    {
        /// <summary>
        /// Encrypts and gets the ciphertext path from provided <paramref name="plaintextRelativePath"/>.
        /// </summary>
        /// <param name="plaintextRelativePath">The relative plaintext path to an item.</param>
        /// <param name="specifics">The specifics.</param>
        /// <returns>A full ciphertext path.</returns>
        public static string GetCiphertextPath(string plaintextRelativePath, FileSystemSpecifics specifics)
        {
            var directoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            return GetCiphertextPath(plaintextRelativePath, specifics, directoryId);
        }

        /// <summary>
        /// Decrypts and gets the plaintext path from provided <paramref name="ciphertextPath"/>.
        /// </summary>
        /// <param name="ciphertextPath">The relative plaintext path to an item.</param>
        /// <param name="specifics">The specifics.</param>
        /// <returns>A relative plaintext path.</returns>
        public static string? GetPlaintextPath(string ciphertextPath, FileSystemSpecifics specifics)
        {
            var directoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            return GetPlaintextPath(ciphertextPath, specifics, directoryId);
        }

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
            {
                if (plaintextRelativePath.StartsWith(Path.DirectorySeparatorChar))
                    return specifics.ContentFolder.Id + plaintextRelativePath;

                return Path.Combine(specifics.ContentFolder.Id, plaintextRelativePath);
            }

            var finalPath = specifics.ContentFolder.Id;
            foreach (var namePart in plaintextRelativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Encrypt the name part
                var ciphertextName = EncryptName(namePart, finalPath, specifics, expendableDirectoryId);

                // Combine the final path
                finalPath = Path.Combine(finalPath, ciphertextName);
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
            // Use a relative path in both cases
            ciphertextPath = MakeRelative(ciphertextPath, specifics.ContentFolder.Id);

            // Return the (relative) path, if not using name encryption
            if (specifics.Security.NameCrypt is null)
                return ciphertextPath;

            var finalPath = string.Empty;
            var finalCiphertextPath = specifics.ContentFolder.Id;
            foreach (var namePart in ciphertextPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Decrypt the name part
                var plaintextName = DecryptName(namePart, finalCiphertextPath, specifics, expendableDirectoryId);
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
