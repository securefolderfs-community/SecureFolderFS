using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    public abstract class BaseFileSystemValidator<T> : IAsyncValidator<T, IWrapper<IResult>>
    {
        protected FileSystemSpecifics specifics;

        protected BaseFileSystemValidator(FileSystemSpecifics specifics)
        {
            this.specifics = specifics;
        }

        /// <inheritdoc/>
        public abstract Task ValidateAsync(T value, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IWrapper<IResult>> ValidateResultAsync(T value, CancellationToken cancellationToken = default);

        protected async Task ValidateNameAsync(IStorableChild storable, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return;

            var decryptedName = await DecryptNameAsync(storable, cancellationToken);
            if (decryptedName is null)
                throw new CryptographicException("The item name is invalid.");
        }

        protected async Task<string?> DecryptNameAsync(IStorableChild storable, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return null;

            var parentFolder = await storable.GetParentAsync(cancellationToken);
            if (parentFolder is null)
               return null;

            var ciphertextName = storable.Name;
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextName);
            var result = await AbstractPathHelpers.GetDirectoryIdAsync(parentFolder, specifics, directoryId);

            return specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(ciphertextName), result ? directoryId : ReadOnlySpan<byte>.Empty);
        }
    }
}
