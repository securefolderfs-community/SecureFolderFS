using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultUpgrader
{
    internal interface IVaultUpgrader
    {
        // TODO: Parameters are a subject to change
        Task<bool> Upgrade(string vaultPath, BaseVaultKeystore vaultKeystore, BaseVaultConfiguration vaultConfiguration, IPassword password, IProgress<double> progress, CancellationToken cancellationToken);
    }
}
