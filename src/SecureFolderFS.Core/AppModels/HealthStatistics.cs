using OwlCore.Storage;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.AppModels
{
    /// <inheritdoc cref="IHealthStatistics"/>
    public sealed class HealthStatistics : IHealthStatistics
    {
        public HealthStatistics(IFolder vaultFolder)
        {
            FileValidator = new FileValidator(vaultFolder);
            FolderValidator = new FolderValidator(vaultFolder);
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
