﻿using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IHealthStatistics
    {
        /// <summary>
        /// Gets the file health validator associated with this instance.
        /// </summary>
        IAsyncValidator<IFile, IResult>? FileValidator { get; set; }

        /// <summary>
        /// Gets the file content validator for deep file validation.
        /// </summary>
        IAsyncValidator<IFile, IResult>? FileContentValidator { get; set; }

        /// <summary>
        /// Gets the file health validator associated with this instance.
        /// </summary>
        IAsyncValidator<IFolder, IResult>? FolderValidator { get; set; }

        /// <summary>
        /// Gets the file system structure validator.
        /// </summary>
        /// <remarks>
        /// The consumer might provide an implementation of <see cref="IProgress{T}"/> of <see cref="IResult"/> to capture errors in real-time.
        /// <br/>
        /// The reporting of errors, however, is not guaranteed by the implementation and the consuming side should always
        /// account for this and consider using a fallback in the form of the return value.
        /// </remarks>
        IAsyncValidator<(IFolder, IProgress<IResult>?), IResult>? StructureValidator { get; set; }

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
