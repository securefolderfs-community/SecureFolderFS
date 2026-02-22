using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Health;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Validators
{
    /// <summary>
    /// Validates file contents including header and chunk integrity.
    /// </summary>
    public sealed class FileContentValidator : BaseFileSystemValidator<IFile>
    {
        public FileContentValidator(FileSystemSpecifics specifics)
            : base(specifics)
        {
        }

        /// <inheritdoc/>
        public override async Task ValidateAsync(IFile value, CancellationToken cancellationToken = default)
        {
            if (PathHelpers.IsCoreName(value.Name))
                return;

            var result = await HealthHelpers.ValidateFileContentsAsync(value, specifics.Security, cancellationToken);

            if (result.IsIrrecoverable)
                throw new FileHeaderCorruptedException(value.Name);

            if (result.CorruptedChunks.Count > 0)
                throw new FileChunksCorruptedException(value.Name, result.CorruptedChunks);
        }

        /// <inheritdoc/>
        public override async Task<IResult> ValidateResultAsync(IFile value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateAsync(value, cancellationToken).ConfigureAwait(false);
                return Result<StorableType>.Success(StorableType.File);
            }
            catch (Exception ex)
            {
                return Result<IStorable>.Failure(value, ex);
            }
        }
    }

    /// <summary>
    /// Exception thrown when a file header is corrupted and the file is irrecoverable.
    /// </summary>
    public sealed class FileHeaderCorruptedException : CryptographicException
    {
        public FileHeaderCorruptedException(string fileName)
            : base($"File header is corrupted and cannot be recovered: {fileName}")
        {
        }
    }

    /// <summary>
    /// Exception thrown when one or more file chunks are corrupted.
    /// </summary>
    public sealed class FileChunksCorruptedException : CryptographicException
    {
        public IReadOnlyList<long> CorruptedChunks { get; }

        public FileChunksCorruptedException(string fileName, IReadOnlyList<long> corruptedChunks)
            : base($"File has {corruptedChunks.Count} corrupted chunk(s): {fileName}")
        {
            CorruptedChunks = corruptedChunks;
        }
    }
}

