using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Tests.Helpers;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.FileSystemTests
{
    public abstract class BaseFileSystemTests
    {
        protected async Task<IVFSRoot> MountVault(IFileSystem fileSystem, MockVaultOptions? options, params (string, object)[] additionalOptions)
        {
            var (vaultFolder, recoveryKey) = await MockVaultHelpers.CreateVaultLatestAsync(options);

            var vaultService = DI.Service<IVaultService>();
            var vaultManagerService = DI.Service<IVaultManagerService>();
            var unlockContract = await vaultManagerService.RecoverAsync(vaultFolder, recoveryKey);

            // Configure options
            var fileSystemOptions = new Dictionary<string, object>()
            {
                { nameof(VirtualFileSystemOptions.VolumeName), Guid.NewGuid().ToString() }
            };

            foreach (var item in additionalOptions)
            {
                if (item.Item1 == nameof(VirtualFileSystemOptions.VolumeName))
                    fileSystemOptions.Remove(nameof(VirtualFileSystemOptions.VolumeName));

                fileSystemOptions.Add(item.Item1, item.Item2);
            }

            // Create the storage layer
            var contentFolder = await vaultFolder.GetFolderByNameAsync(vaultService.ContentFolderName);
            return await fileSystem.MountAsync(contentFolder, unlockContract, fileSystemOptions);
        }
    }
}
