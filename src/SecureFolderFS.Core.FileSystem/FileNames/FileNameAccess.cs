using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Shared.Enums;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    /// <summary>
    /// Accesses cleartext and ciphertext names of files and folders found on the encrypting file system.
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
        /// Gets cleartext name from associated <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The associated ciphertext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a cleartext representation of the name; otherwise empty.</returns>
        public virtual string GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);

            var nameWithoutExt = Path.GetFileNameWithoutExtension(ciphertextName);
            if (nameWithoutExt.IsEmpty)
                return string.Empty;

            var cleartextName = security.NameCrypt!.DecryptName(nameWithoutExt, directoryId);
            if (cleartextName is null)
                return string.Empty;

            return cleartextName;
        }

        /// <summary>
        /// Gets ciphertext name from associated <paramref name="cleartextName"/>.
        /// </summary>
        /// <param name="cleartextName">The associated cleartext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a ciphertext representation of the name; otherwise empty.</returns>
        public virtual string GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);

            var ciphertextName = security.NameCrypt!.EncryptName(cleartextName, directoryId);
            return Path.ChangeExtension(ciphertextName, Constants.Names.ENCRYPTED_FILE_EXTENSION);
        }
    }
}
