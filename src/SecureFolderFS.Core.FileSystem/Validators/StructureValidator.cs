using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T, TResult}"/>
    internal sealed class StructureValidator : BaseFileSystemValidator<(IFolder, IProgress<IResult>?)>
    {
        private readonly IAsyncValidator<IFile, IResult> _fileValidator;
        private readonly IAsyncValidator<IFolder, IResult> _folderValidator;

        public StructureValidator(FileSystemSpecifics specifics, IAsyncValidator<IFile, IResult> fileValidator, IAsyncValidator<IFolder, IResult> folderValidator)
            : base(specifics)
        {
            _fileValidator = fileValidator;
            _folderValidator = folderValidator;
        }

        /// <inheritdoc/>
        public override Task ValidateAsync((IFolder, IProgress<IResult>?) value, CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync((IFolder, IProgress<IResult>?) value, CancellationToken cancellationToken = default)
        {
            var scannedFolder = value.Item1;
            var reporter = value.Item2;

            var folderResult = await _folderValidator.ValidateResultAsync(scannedFolder, cancellationToken).ConfigureAwait(false);
            reporter?.Report(folderResult);

            await foreach (var item in scannedFolder.GetItemsAsync(StorableType.All, cancellationToken).ConfigureAwait(false))
            {
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                if (item is IChildFile file)
                {
                    var result = await _fileValidator.ValidateResultAsync(file, cancellationToken).ConfigureAwait(false);
                    reporter?.Report(result);
                }

                if (folderResult.Successful)
                {
                    var nameResult = await ValidateNameResultAsync(item, cancellationToken).ConfigureAwait(false);
                    reporter?.Report(nameResult);
                }
            }

            // TODO: Return appropriate result
            return Result.Success;
        }
    }
}
