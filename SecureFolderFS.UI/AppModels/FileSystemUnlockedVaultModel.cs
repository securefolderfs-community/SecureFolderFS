using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IUnlockedVaultModel"/>
    internal sealed class FileSystemUnlockedVaultModel : IUnlockedVaultModel
    {
        private readonly IVirtualFileSystem _virtualFileSystem;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public IVaultStatisticsModel VaultStatisticsModel { get; }

        public FileSystemUnlockedVaultModel(IVirtualFileSystem virtualFileSystem, IVaultStatisticsModel vaultStatisticsModel)
        {
            _virtualFileSystem = virtualFileSystem;
            RootFolder = virtualFileSystem.RootFolder;
            VaultStatisticsModel = vaultStatisticsModel;
        }

        /// <inheritdoc/>
        public async Task LockAsync()
        {
            await _virtualFileSystem.CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }
    }
}
