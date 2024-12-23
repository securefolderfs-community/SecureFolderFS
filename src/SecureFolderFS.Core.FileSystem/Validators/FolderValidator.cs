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
    public sealed class FolderValidator : BaseFileSystemValidator<IFolder>
    {
        public FolderValidator(FileSystemSpecifics specifics)
            : base(specifics)
        {
        }

        /// <inheritdoc/>
        public override async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            // Check if Directory ID exists
            var directoryIdFile = await value.GetFileByNameAsync(Core.FileSystem.Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken);

            // Check the size
            await using var stream = await directoryIdFile.OpenReadAsync(cancellationToken);
            if (stream.Length != Core.FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new FormatException($"The Directory ID size is invalid. Expected: {Core.FileSystem.Constants.DIRECTORY_ID_SIZE}; Got: {stream.Length}.");

            // Check the name
            if (value is IChildFolder childFolder)
                await ValidateNameAsync(childFolder, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IWrapper<IResult>> ValidateResultAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken);
                return new Wrapper<IResult>(Result.Success);
            }
            catch (Exception ex)
            {
                _ = ex;
                return new Wrapper<IResult>(Result.Failure(ex)); // TODO: Return appropriate IHealthResult based on the exception
            }
        }
    }
}
