using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public abstract class BaseFileSystemValidator<T> : IAsyncValidator<T, IResult>
    {
        protected FileSystemSpecifics specifics;

        protected BaseFileSystemValidator(FileSystemSpecifics specifics)
        {
            this.specifics = specifics;
        }

        /// <inheritdoc/>
        public abstract Task ValidateAsync(T value, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IResult> ValidateResultAsync(T value, CancellationToken cancellationToken = default);

        protected async Task ValidateNameAsync(IStorableChild storable, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return;

            var decryptedName = await DecryptNameAsync(storable, cancellationToken).ConfigureAwait(false);
            if (decryptedName is null)
                throw new FormatException("The item name is invalid.");
        }

        protected async Task<IResult> ValidateNameResultAsync(IStorableChild storable, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateNameAsync(storable, cancellationToken).ConfigureAwait(false);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result<IStorable>.Failure(storable, ex);
            }
        }

        protected async Task<string?> DecryptNameAsync(IStorableChild storable, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return null;

            var parentFolder = await storable.GetParentAsync(cancellationToken).ConfigureAwait(false);
            if (parentFolder is null)
                return null;

            var ciphertextName = storable.Name;
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextName);

            try
            {
                var result = await AbstractPathHelpers.GetDirectoryIdAsync(parentFolder, specifics, directoryId, cancellationToken).ConfigureAwait(false);
                return specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(ciphertextName), result ? directoryId : ReadOnlySpan<byte>.Empty);
            }
            catch (FileNotFoundException)
            {
                // We want to suppress FileNotFoundException that might be raised when the DirectoryID file is not found.
                // This case should be already handled in the folder validator

                // Return an empty string to prevent raising exceptions due to the name being null
                return string.Empty;
            }
        }
    }
}
