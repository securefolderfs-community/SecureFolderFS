using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T}"/>
    internal sealed class StructureContentsValidator : BaseFileSystemValidator<(IFolder, IProgress<IResult>?)>
    {
        private readonly IAsyncValidator<IFolder, IResult> _folderValidator;
        private readonly IAsyncValidator<IFile, IResult> _fileContentValidator;

        public StructureContentsValidator(FileSystemSpecifics specifics, IAsyncValidator<IFile, IResult> fileContentValidator, IAsyncValidator<IFolder, IResult> folderValidator)
            : base(specifics)
        {
            _folderValidator = folderValidator;
            _fileContentValidator = fileContentValidator;
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

            await foreach (var item in scannedFolder.GetItemsAsync(StorableType.All, cancellationToken).ConfigureAwait(false))
            {
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                switch (item)
                {
                    case IFile file:
                    {
                        var fileResult = await _fileContentValidator.ValidateResultAsync(file, cancellationToken).ConfigureAwait(false);
                        var nameResult = folderResult.Successful
                            ? await ValidateNameResultAsync(item, cancellationToken).ConfigureAwait(false)
                            : Result<StorableType>.Success(StorableType.File); // Assuming success on name check here when a parent is broken will skip the checks

                        if (!fileResult.Successful && !nameResult.Successful)
                        {
                            // Both have failed, report aggregated failure
                            reporter?.Report(Result<(IResult, IResult)>.Failure((fileResult, nameResult)));
                        }
                        else if (!fileResult.Successful)
                        {
                            // Only a file has failed, report file failure
                            reporter?.Report(fileResult);
                        }
                        else if (!nameResult.Successful)
                        {
                            // Only a name has failed, report name failure
                            reporter?.Report(nameResult);
                        }
                        else
                        {
                            // Both have succeeded, report file result success
                            reporter?.Report(fileResult);
                        }

                        break;
                    }

                    case IFolder:
                    {
                        if (folderResult.Successful)
                        {
                            var nameResult = await ValidateNameResultAsync(item, cancellationToken).ConfigureAwait(false);
                            reporter?.Report(nameResult);
                        }

                        break;
                    }
                }
            }

            return folderResult;
        }
    }
}
