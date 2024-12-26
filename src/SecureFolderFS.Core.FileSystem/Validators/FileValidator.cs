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
        private IResult<StorableType> FileSuccess { get; } = Result<StorableType>.Success(StorableType.File);

        public FileValidator(FileSystemSpecifics specifics)
            : base(specifics)
        {
        }

        /// <inheritdoc/>
        public override Task ValidateAsync(IFile value, CancellationToken cancellationToken = default)
        {
            if (PathHelpers.IsCoreFile(value.Name))
                return Task.CompletedTask;

            // TODO: Implement file validation (invalid chunks, checksum mismatch, etc...?)
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync(IFile value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken).ConfigureAwait(false);
                return FileSuccess;
            }
            catch (Exception ex)
            {
                return Result<IStorable>.Failure(value, ex);
            }
        }
    }
}
