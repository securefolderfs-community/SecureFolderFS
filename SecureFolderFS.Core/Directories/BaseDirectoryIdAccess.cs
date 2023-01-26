using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Directories;
using System;
using System.IO;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public abstract class BaseDirectoryIdAccess : IDirectoryIdAccess
    {
        protected readonly IDirectoryIdStreamAccess directoryIdStreamAccess;
        protected readonly IFileSystemStatistics? fileSystemStatistics;
        protected readonly IFileSystemHealthStatistics? fileSystemHealthStatistics;

        protected BaseDirectoryIdAccess(IDirectoryIdStreamAccess directoryIdStreamAccess, IFileSystemStatistics? fileSystemStatistics, IFileSystemHealthStatistics? fileSystemHealthStatistics)
        {
            this.directoryIdStreamAccess = directoryIdStreamAccess;
            this.fileSystemStatistics = fileSystemStatistics;
            this.fileSystemHealthStatistics = fileSystemHealthStatistics;
        }

        /// <inheritdoc/>
        public virtual bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId)
        {
            // Check if directoryId Span is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                return false;

            // Check if path is empty
            if (ciphertextPath.Length == 0)
            {
                fileSystemHealthStatistics?.ReportInvalidPath(ciphertextPath);
                return false;
            }

            // Update stats
            fileSystemStatistics?.NotifyDirectoryIdAccess();

            // Open stream to directory id
            using var fileStream = directoryIdStreamAccess.OpenDirectoryIdStream(ciphertextPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (fileStream is null)
            {
                fileSystemHealthStatistics?.ReportDirectoryIdNotFound(ciphertextPath);
                return false;
            }

            // Check if stream length is correct
            if (fileStream.Length < FileSystem.Constants.DIRECTORY_ID_SIZE)
            {
                fileSystemHealthStatistics?.ReportDirectoryIdInvalid(ciphertextPath);
                return false;
            }

            // Read directory ID from stream
            var read = fileStream.Read(directoryId);
            if (read < FileSystem.Constants.DIRECTORY_ID_SIZE)
            {
                fileSystemHealthStatistics?.ReportDirectoryIdInvalid(ciphertextPath);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public virtual bool SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            // Check if directoryId Span is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                return false;

            // Open stream to directory id
            using var fileStream = directoryIdStreamAccess.OpenDirectoryIdStream(ciphertextPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
            if (fileStream is null)
            {
                fileSystemHealthStatistics?.ReportDirectoryIdNotFound(ciphertextPath);
                return false;
            }

            // Write directory id to stream
            fileStream.Write(directoryId);

            return true;
        }

        /// <inheritdoc/>
        public virtual void RemoveDirectoryId(string ciphertextPath)
        {
        }
    }
}
