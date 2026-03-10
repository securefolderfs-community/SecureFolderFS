using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T,TResult}"/>
    internal sealed class StructureValidator : BaseFileSystemValidator<(IFolder, IProgress<IResult>?)>
    {
        private readonly IAsyncValidator<IFolder, IResult> _folderValidator;

        public StructureValidator(FileSystemSpecifics specifics, IAsyncValidator<IFolder, IResult> folderValidator)
            : base(specifics)
        {
            _folderValidator = folderValidator;
        }

        /// <inheritdoc/>
        public override Task ValidateAsync((IFolder, IProgress<IResult>?) value, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync((IFolder, IProgress<IResult>?) value, CancellationToken cancellationToken = default)
        {
            var scannedFolder = value.Item1;
            var reporter = value.Item2;

            var folderResult = await _folderValidator.ValidateResultAsync(scannedFolder, cancellationToken).ConfigureAwait(false);
            reporter?.Report(folderResult);

            if (!folderResult.Successful)
                return folderResult;

            await foreach (var item in scannedFolder.GetItemsAsync(StorableType.All, cancellationToken).ConfigureAwait(false))
            {
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                var nameResult = await ValidateNameResultAsync(item, cancellationToken).ConfigureAwait(false);
                reporter?.Report(nameResult);
            }

            return Result.Success;
        }
    }
}
