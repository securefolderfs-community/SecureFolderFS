using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class VaultModelExtensions
    {
        public static async Task<IChildFolder> GetContentFolderAsync(this IVaultModel vaultModel, CancellationToken cancellationToken = default)
        {
            var vaultService = DI.Service<IVaultService>();
            return await vaultModel.Folder.GetFolderByNameAsync(vaultService.ContentFolderName, cancellationToken);
        }
    }
}
