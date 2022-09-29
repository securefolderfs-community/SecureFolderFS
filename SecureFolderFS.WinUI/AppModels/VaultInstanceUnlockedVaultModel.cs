using SecureFolderFS.Core.Instance;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IUnlockedVaultModel"/>
    internal sealed class VaultInstanceUnlockedVaultModel : IUnlockedVaultModel
    {
        private readonly IVaultInstance _vaultInstance;

        /// <inheritdoc/>
        public IFolder UnlockedFolder { get; }

        /// <inheritdoc/>
        public IVaultStatisticsModel VaultStatisticsModel { get; }

        public VaultInstanceUnlockedVaultModel(IVaultInstance vaultInstance, IFolder unlockedFolder, IVaultStatisticsModel vaultStatisticsModel)
        {
            _vaultInstance = vaultInstance;
            UnlockedFolder = unlockedFolder;
            VaultStatisticsModel = vaultStatisticsModel;
        }

        /// <inheritdoc/>
        public Task LockAsync()
        {
            _vaultInstance.Dispose();
            return Task.CompletedTask;
        }
    }
}
