using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public sealed class FileValidator : BaseFileSystemValidator<IFile>
    {
        public FileValidator(FileSystemSpecifics specifics)
            : base(specifics)
        {
        }

        /// <inheritdoc/>
        public override async Task ValidateAsync(IFile value, CancellationToken cancellationToken = default)
        {
            if (PathHelpers.IsCoreFile(value.Name))
                return;

            // Check the name
            if (value is IChildFile childFile)
                await ValidateNameAsync(childFile, cancellationToken);

            // TODO: Implement file validation (invalid chunks, checksum mismatch, etc...?)
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync(IFile value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}
