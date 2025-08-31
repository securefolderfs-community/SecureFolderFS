using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class VaultHelpers
    {
        public static async Task<IChildFolder> GetOrCreateContentFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            var vaultService = DI.Service<IVaultService>();
            var contentFolder = await SafetyHelpers.NoFailureAsync(async () => await GetContentFolderAsync(vaultFolder, cancellationToken));

            if (vaultFolder is not IModifiableFolder modifiableFolder)
                throw new UnauthorizedAccessException("The vault folder is not modifiable.");

            return contentFolder ?? await modifiableFolder.CreateFolderAsync(vaultService.ContentFolderName, false, cancellationToken);
        }

        public static async Task<IChildFolder> GetContentFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            var vaultService = DI.Service<IVaultService>();
            return await vaultFolder.GetFolderByNameAsync(vaultService.ContentFolderName, cancellationToken);
        }
    }
}
