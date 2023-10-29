using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IVaultLifecycle"/>
    internal sealed class VaultLifetimeModel : IVaultLifecycle
    {
        private readonly IVirtualFileSystem _virtualFileSystem;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public IVaultStatisticsModel VaultStatisticsModel { get; }

        /// <inheritdoc/>
        public VaultOptions VaultOptions { get; }

        public VaultLifetimeModel(IVirtualFileSystem virtualFileSystem, IVaultStatisticsModel vaultStatisticsModel, VaultOptions vaultOptions)
        {
            _virtualFileSystem = virtualFileSystem;
            RootFolder = virtualFileSystem.RootFolder;
            VaultStatisticsModel = vaultStatisticsModel;
            VaultOptions = vaultOptions;
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
