using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultAccess
{
    // TODO: Needs docs
    internal interface IVaultWriter
    {
        Task WriteAsync(VaultKeystoreDataModel? keystoreDataModel, VaultConfigurationDataModel? configDataModel, VaultAuthenticationDataModel? authDataModel, CancellationToken cancellationToken);
    }
}
