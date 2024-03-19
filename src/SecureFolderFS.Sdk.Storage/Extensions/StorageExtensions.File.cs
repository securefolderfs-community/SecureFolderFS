using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <inheritdoc cref="IFileExtended.OpenStreamAsync(FileAccess, FileShare, CancellationToken)"/>
        public static async Task<Stream> OpenStreamAsync(this IFile file, FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            if (file is IFileExtended fileExtended)
                return await fileExtended.OpenStreamAsync(access, share, cancellationToken);

            // TODO: Check if the file inherits from ILockableStorable and ensure a disposable handle to it via Stream bridge
            return await file.OpenStreamAsync(access, cancellationToken);
        }

        /// <returns>If successful, returns a <see cref="Stream"/>; otherwise null.</returns>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        public static async Task<Stream?> TryOpenStreamAsync(this IFile file, FileAccess access, CancellationToken cancellationToken = default)
        {
            try
            {
                return await file.OpenStreamAsync(access, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If successful, returns a <see cref="Stream"/>; otherwise null.</returns>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        public static async Task<Stream?> TryOpenStreamAsync(this IFile file, FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            try
            {
                return await OpenStreamAsync(file, access, share, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region With Result

        /// <returns>Value is <see cref="IResult"/> depending on whether the stream was successfully opened on the file.</returns>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        public static async Task<IResult<Stream?>> OpenStreamWithResultAsync(this IFile file, FileAccess access, CancellationToken cancellationToken = default)
        {
            try
            {
                return Result<Stream?>.Success(await file.OpenStreamAsync(access, cancellationToken));
            }
            catch (Exception ex)
            {
                return Result<Stream?>.Failure(ex);
            }
        }

        /// <returns>Value is <see cref="IResult"/> depending on whether the stream was successfully opened on the file.</returns>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        public static async Task<IResult<Stream?>> OpenStreamWithResultAsync(this IFile file, FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            try
            {
                if (file is IFileExtended fileExtended)
                    return Result<Stream?>.Success(await fileExtended.OpenStreamAsync(access, share, cancellationToken));

                // TODO: Check if the file inherits from ILockableStorable and ensure a disposable handle to it via Stream bridge
                return Result<Stream?>.Success(await file.OpenStreamAsync(access, cancellationToken));
            }
            catch (Exception ex)
            {
                return Result<Stream?>.Failure(ex);
            }
        }

        #endregion
    }
}
