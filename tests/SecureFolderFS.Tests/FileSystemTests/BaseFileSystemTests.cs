using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Tests.Helpers;

namespace SecureFolderFS.Tests.FileSystemTests
{
    public abstract class BaseFileSystemTests
    {
        protected async Task<IVFSRoot> MountVault(IFileSystem fileSystem)
        {
            var (vaultFolder, recoveryKey) = await MockVaultHelpers.CreateVaultLatestAsync();

            var vaultService = DI.Service<IVaultService>();
            var vaultManagerService = DI.Service<IVaultManagerService>();
            var unlockContract = await vaultManagerService.RecoverAsync(vaultFolder, recoveryKey);

            // Configure options
            var options = new Dictionary<string, object>()
            {
                { nameof(FileSystemOptions.VolumeName), Guid.NewGuid().ToString() }
            };

            // Create the storage layer
            var contentFolder = await vaultFolder.GetFolderByNameAsync(vaultService.ContentFolderName);
            return await fileSystem.MountAsync(contentFolder, unlockContract, options);
        }
    }
}
