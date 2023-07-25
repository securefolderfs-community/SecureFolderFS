using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultAccess
{
    // TODO: Needs docs
    internal interface IVaultReader
    {
        Task<(VaultKeystoreDataModel, VaultConfigurationDataModel)> ReadAsync(CancellationToken cancellationToken);
    }
}
