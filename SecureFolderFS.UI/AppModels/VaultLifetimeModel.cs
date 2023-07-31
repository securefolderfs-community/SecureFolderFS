using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IVaultLifetimeModel"/>
    internal sealed class VaultLifetimeModel : IVaultLifetimeModel
    {
        private readonly IVirtualFileSystem _virtualFileSystem;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public IVaultStatisticsModel VaultStatisticsModel { get; }

        /// <inheritdoc/>
        public VaultInfoModel VaultInfoModel { get; }

        public VaultLifetimeModel(IVirtualFileSystem virtualFileSystem, IVaultStatisticsModel vaultStatisticsModel, VaultInfoModel vaultInfoModel)
        {
            _virtualFileSystem = virtualFileSystem;
            RootFolder = virtualFileSystem.RootFolder;
            VaultStatisticsModel = vaultStatisticsModel;
            VaultInfoModel = vaultInfoModel;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await _virtualFileSystem.CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _virtualFileSystem.CloseAsync(FileSystemCloseMethod.CloseForcefully).Wait();
        }
    }
}
