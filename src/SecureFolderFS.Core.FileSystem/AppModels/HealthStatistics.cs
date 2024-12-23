using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;

namespace SecureFolderFS.Core.FileSystem.AppModels
{
    /// <inheritdoc cref="IHealthStatistics"/>
    public sealed class HealthStatistics : IHealthStatistics
    {
        /// <inheritdoc/>
        public IAsyncValidator<IFile, IWrapper<IResult>>? FileValidator { get; set; }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder, IWrapper<IResult>>? FolderValidator { get; set; }

        /// <inheritdoc/>
        public IProgress<string>? DirectoryIdNotFound { get; set; }

        /// <inheritdoc/>
        public IProgress<string>? DirectoryIdInvalid { get; set; }

        /// <inheritdoc/>
        public IProgress<string>? InvalidPath { get; set; }
    }
}
