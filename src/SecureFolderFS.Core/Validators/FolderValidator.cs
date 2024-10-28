using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    public sealed class FolderValidator : IAsyncValidator<IFolder>
    {
        private readonly IFolder _vaultFolder;

        public FolderValidator(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            // Check if Directory ID exists
            var directoryIdFile = await value.GetFileByNameAsync(Core.FileSystem.Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken);

            // Check the size
            await using var stream = await directoryIdFile.OpenReadAsync(cancellationToken);
            if (stream.Length != Core.FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new FormatException($"The Directory ID size is invalid. Expected: {Core.FileSystem.Constants.DIRECTORY_ID_SIZE}, got: {stream.Length}.");
        }
    }
}
