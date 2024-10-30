using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public sealed class FileValidator : IAsyncValidator<IFile, IResult>
    {
        private readonly IFolder _vaultFolder;

        public FileValidator(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public Task ValidateAsync(IFile value, CancellationToken cancellationToken = default)
        {
            // TODO: Implement file validation (invalid chunks, invalid name, checksum mismatch, etc...?)
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IResult> ValidateResultAsync(IFile value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _ = ex;
                return Result.Failure(ex); // TODO: Return appropriate IHealthResult based on the exception
            }
        }
    }
}
