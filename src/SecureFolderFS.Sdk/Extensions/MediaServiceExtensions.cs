using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

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
    }
}
