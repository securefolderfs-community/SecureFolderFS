using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.FileNames;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileNames
{
    /// <inheritdoc cref="IFileNameAccess"/>
    internal class FileNameAccess : IFileNameAccess
    {
        protected readonly Security security;
        protected readonly IFileSystemStatistics? fileSystemStatistics;

        public FileNameAccess(Security security, IFileSystemStatistics? fileSystemStatistics)
        {
            this.security = security;
            this.fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        public virtual string GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            fileSystemStatistics?.NotifyFileNameAccess();

            var nameWithoutExt = Path.GetFileNameWithoutExtension(ciphertextName);
            if (nameWithoutExt.IsEmpty)
                return string.Empty;

            var cleartextName = security.NameCrypt!.DecryptName(nameWithoutExt, directoryId);
            if (cleartextName is null)
                return string.Empty;

            return cleartextName;
        }

        /// <inheritdoc/>
        public virtual string GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            fileSystemStatistics?.NotifyFileNameAccess();

            var ciphertextName = security.NameCrypt!.EncryptName(cleartextName, directoryId);
            return Path.ChangeExtension(ciphertextName, Constants.Vault.ENCRYPTED_FILE_EXTENSION);
        }
    }
}
