using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public sealed class FolderValidator : IAsyncValidator<IFolder, IResult>
    {
        private readonly IFolder _contentFolder;

        public FolderValidator(IFolder contentFolder)
        {
            _contentFolder = contentFolder;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            // TODO: Do not validate if no file name encryption is set
            // TODO: Validate the name of the folder itself

            // Check if Directory ID exists
            var directoryIdFile = await value.GetFileByNameAsync(Core.FileSystem.Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken);

            // Check the size
            await using var stream = await directoryIdFile.OpenReadAsync(cancellationToken);
            if (stream.Length != Core.FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new FormatException($"The Directory ID size is invalid. Expected: {Core.FileSystem.Constants.DIRECTORY_ID_SIZE}, got: {stream.Length}.");
        }

        /// <inheritdoc/>
        public async Task<IResult> ValidateResultAsync(IFolder value, CancellationToken cancellationToken = default)
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
