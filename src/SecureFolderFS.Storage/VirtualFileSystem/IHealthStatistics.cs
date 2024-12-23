using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IHealthStatistics
    {
        /// <summary>
        /// Gets the file health validator associated with this instance.
        /// </summary>
        IAsyncValidator<IFile, IWrapper<IResult>>? FileValidator { get; set; }

        /// <summary>
        /// Gets the file health validator associated with this instance.
        /// </summary>
        IAsyncValidator<IFolder, IWrapper<IResult>>? FolderValidator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports when the file containing DirectoryID was not found.
        /// </summary>
        IProgress<string>? DirectoryIdNotFound { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports when the file contents containing DirectoryID are invalid.
        /// </summary>
        IProgress<string>? DirectoryIdInvalid { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IProgress{T}"/> that reports when the file name could not be encrypted or decrypted.
        /// </summary>
        IProgress<string>? InvalidPath { get; set; }
    }
}
