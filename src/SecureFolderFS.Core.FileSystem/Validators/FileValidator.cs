using OwlCore.Storage;
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
            // Check the name
            if (value is IChildFile childFile)
                await ValidateNameAsync(childFile, cancellationToken);

            // TODO: Implement file validation (invalid chunks, checksum mismatch, etc...?)
        }

        /// <inheritdoc/>
        public override async Task<IWrapper<IResult>> ValidateResultAsync(IFile value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken);
                return new Wrapper<IResult>(Result.Success);
            }
            catch (Exception ex)
            {
                _ = ex;
                // TODO: Return a ViewModel
                return new Wrapper<IResult>(Result.Failure(ex)); // TODO: Return appropriate IHealthResult based on the exception
            }
        }
    }
}
