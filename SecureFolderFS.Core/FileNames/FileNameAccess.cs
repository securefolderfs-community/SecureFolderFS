using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileNames
{
    /// <inheritdoc cref="IFileNameAccess"/>
    internal class FileNameAccess : IFileNameAccess
    {
        protected readonly ISecurity security;
        protected readonly IFileSystemStatsTracker? statsTracker;

        public FileNameAccess(ISecurity security, IFileSystemStatsTracker? statsTracker)
        {
            this.security = security;
            this.statsTracker = statsTracker;
        }

        /// <inheritdoc/>
        public virtual ReadOnlySpan<char> GetCleartextName(ReadOnlySpan<char> ciphertextName, DirectoryId directoryId)
        {
            statsTracker?.AddFileNameAccess();

            var nameWithoutExt = Path.GetFileNameWithoutExtension(ciphertextName);
            if (nameWithoutExt.IsEmpty)
                return ReadOnlySpan<char>.Empty;

            var cleartextName = security.NameCrypt!.DecryptName(nameWithoutExt, directoryId.Id);
            if (cleartextName is null)
                return ReadOnlySpan<char>.Empty;

            return cleartextName;
        }

        /// <inheritdoc/>
        public virtual ReadOnlySpan<char> GetCiphertextName(ReadOnlySpan<char> cleartextName, DirectoryId directoryId)
        {
            statsTracker?.AddFileNameAccess();

            var ciphertextName = security.NameCrypt!.EncryptName(cleartextName, directoryId);
            return Path.ChangeExtension(ciphertextName, Constants.ENCRYPTED_FILE_EXTENSION);
        }
    }
}
