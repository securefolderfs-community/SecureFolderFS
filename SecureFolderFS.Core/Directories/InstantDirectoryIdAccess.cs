using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.Directories;
using System;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public sealed class InstantDirectoryIdAccess : IDirectoryIdAccess
    {
        private readonly IFileSystemStatistics? _fileSystemStatistics;

        public InstantDirectoryIdAccess(IFileSystemStatistics? fileSystemStatistics)
        {
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        public bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId)
        {
            // Check if directoryId is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The size of {nameof(directoryId)} was too small.");

            // Check if the ciphertext path is empty
            if (string.IsNullOrEmpty(ciphertextPath))
                throw new ArgumentException($"The {nameof(ciphertextPath)} was empty.");

            // Update statistics
            _fileSystemStatistics?.NotifyDirectoryIdAccess();

            return false;
        }

        /// <inheritdoc/>
        public void SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            // Check if directoryId is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The size of {nameof(directoryId)} was too small.");

            // Check if the ciphertext path is empty
            if (string.IsNullOrEmpty(ciphertextPath))
                throw new ArgumentException($"The {nameof(ciphertextPath)} was empty.");
        }

        /// <inheritdoc/>
        public void RemoveDirectoryId(string ciphertextPath)
        {
            // Check if the ciphertext path is empty
            if (string.IsNullOrEmpty(ciphertextPath))
                throw new ArgumentException($"The {nameof(ciphertextPath)} was empty.");
        }
    }
}
