using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class VaultFileSystemServiceExtensions
    {
        public static async Task<IFileSystem> GetBestFileSystemAsync(this IVaultFileSystemService vaultFileSystemService, CancellationToken cancellationToken = default)
        {
            var vaultService = DI.Service<IVaultService>();
            var settingsService = DI.Service<ISettingsService>();

            IFileSystem? lastBest = null;
            await foreach (var item in vaultFileSystemService.GetFileSystemsAsync(cancellationToken))
            {
                if (item.Id == settingsService.UserSettings.PreferredFileSystemId)
                {
                    if (await item.GetStatusAsync(cancellationToken) == FileSystemAvailability.Available)
                        return item;
                }
                else
                {
                    if (lastBest is null && await item.GetStatusAsync(cancellationToken) == FileSystemAvailability.Available)
                        lastBest = item;
                }
            }

            if (lastBest is null)
                throw new NotSupportedException("No supported adapters found.");

            return lastBest;
        }
    }
}
