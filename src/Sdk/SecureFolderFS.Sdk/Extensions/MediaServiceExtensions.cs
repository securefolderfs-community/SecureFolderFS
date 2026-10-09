using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class MediaServiceExtensions
    {
        public static async Task<IImageStream?> TryGenerateThumbnailAsync(this IMediaService mediaService,
            IFile file,
            TypeHint typeHint = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await mediaService.GenerateThumbnailAsync(file, typeHint, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return null;
            }
        }

        /// <inheritdoc cref="IMediaService.GetSystemIconAsync"/>
        public static async Task<IImage?> TryGetSystemIconAsync(this IMediaService mediaService,
            string itemName,
            bool isFolder,
            uint size = 64u,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await mediaService.GetSystemIconAsync(itemName, isFolder, size, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return null;
            }
        }
    }
}
