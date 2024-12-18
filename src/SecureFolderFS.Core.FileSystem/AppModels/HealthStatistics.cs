using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Validators;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem.AppModels
{
    /// <inheritdoc cref="IHealthStatistics"/>
    public sealed class HealthStatistics : IHealthStatistics
    {
        public HealthStatistics(IFolder contentFolder)
        {
            FileValidator = new FileValidator(contentFolder);
            FolderValidator = new FolderValidator(contentFolder);
        }

        /// <inheritdoc/>
        public IAsyncValidator<IFile, IResult> FileValidator { get; }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder, IResult> FolderValidator { get; }

        /// <inheritdoc/>
        public IProgress<string>? DirectoryIdNotFound { get; set; }

        /// <inheritdoc/>
        public IProgress<string>? DirectoryIdInvalid { get; set; }

        /// <inheritdoc/>
        public IProgress<string>? InvalidPath { get; set; }
    }
}
