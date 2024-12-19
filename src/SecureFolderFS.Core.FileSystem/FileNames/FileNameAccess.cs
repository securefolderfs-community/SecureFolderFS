using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    /// <summary>
    /// Accesses plaintext and ciphertext names of files and folders found on the encrypting file system.
    /// </summary>
    internal class FileNameAccess
    {
        protected readonly Security security;
        protected readonly IFileSystemStatistics statistics;

        public FileNameAccess(Security security, IFileSystemStatistics statistics)
        {
            this.security = security;
            this.statistics = statistics;
        }

        /// <summary>
        /// Gets plaintext name from associated <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The associated ciphertext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a plaintext representation of the name; otherwise empty.</returns>
        public virtual string GetPlaintextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);

            var nameWithoutExt = Path.GetFileNameWithoutExtension(ciphertextName);
            if (nameWithoutExt.IsEmpty)
                return string.Empty;

            var plaintextName = security.NameCrypt!.DecryptName(nameWithoutExt, directoryId);
            if (plaintextName is null)
                return string.Empty;

            return plaintextName;
        }

        /// <summary>
        /// Gets ciphertext name from associated <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The associated plaintext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a ciphertext representation of the name; otherwise empty.</returns>
        public virtual string GetCiphertextName(ReadOnlySpan<char> plaintextName, ReadOnlySpan<byte> directoryId)
        {
            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);

            var ciphertextName = security.NameCrypt!.EncryptName(plaintextName, directoryId);
            return Path.ChangeExtension(ciphertextName, Constants.Names.ENCRYPTED_FILE_EXTENSION);
        }
    }
}
