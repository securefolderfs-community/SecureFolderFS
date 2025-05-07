using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class RecycleBinServiceExtensions
    {
        /// <inheritdoc cref="IRecycleBinService.GetRecycleBinAsync"/>
        public static async Task<IRecycleBinFolder?> TryGetRecycleBinAsync(this IRecycleBinService recycleBinService, IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            try
            {
                return await recycleBinService.GetRecycleBinAsync(vfsRoot, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="IRecycleBinService.GetOrCreateRecycleBinAsync"/>
        public static async Task<IRecycleBinFolder?> TryGetOrCreateRecycleBinAsync(this IRecycleBinService recycleBinService, IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            try
            {
                return await recycleBinService.GetOrCreateRecycleBinAsync(vfsRoot, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="IRecycleBinService.RecalculateSizesAsync"/>
        public static async Task<bool> TryRecalculateSizesAsync(this IRecycleBinService recycleBinService, IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            try
            {
                await recycleBinService.RecalculateSizesAsync(vfsRoot, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
