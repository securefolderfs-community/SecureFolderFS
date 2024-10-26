using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IHealthStatistics
    {
        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports when the file containing DirectoryID was not found.
        /// </summary>
        IProgress<string>? DirectoryIdNotFound { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports when the file contents containing DirectoryID are invalid.
        /// </summary>
        IProgress<string>? DirectoryIdInvalid { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports when the file name could not be encrypted or decrypted.
        /// </summary>
        IProgress<string>? InvalidPath { get; set; }
    }
}
