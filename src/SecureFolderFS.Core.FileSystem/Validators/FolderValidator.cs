using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public sealed class FolderValidator : BaseFileSystemValidator<IFolder>
    {
        private IResult<StorableType> FolderSuccess { get; } = Result<StorableType>.Success(StorableType.Folder);

        public FolderValidator(FileSystemSpecifics specifics)
            : base(specifics)
        {
        }

        /// <inheritdoc/>
        public override async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            try
            {
                if (PathHelpers.IsCoreFile(value.Name))
                    return;

                // Check if Directory ID exists
                var directoryIdFile = await value.GetFileByNameAsync(Core.FileSystem.Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken).ConfigureAwait(false);

                // Check the size
                await using var stream = await directoryIdFile.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                if (stream.Length != Core.FileSystem.Constants.DIRECTORY_ID_SIZE)
                    throw new EndOfStreamException($"The Directory ID size is invalid. Expected: {Core.FileSystem.Constants.DIRECTORY_ID_SIZE}; Got: {stream.Length}.");
            }
            catch (Exception ex)
            {
                // TODO: Consider using the original exception to preserve stacktrace (in such case, also change IVaultFileSystemService)
                throw new AggregateException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken).ConfigureAwait(false);
                return FolderSuccess;
            }
            catch (Exception ex)
            {
                return Result<IStorable>.Failure(value, ex);
            }
        }
    }
}
