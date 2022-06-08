using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.VaultLoader.VaultConfiguration
{
    internal sealed class VaultConfigurationLoaderFactory
    {
        private readonly VaultVersion _vaultVersion;

        public VaultConfigurationLoaderFactory(VaultVersion vaultVersion)
        {
            _vaultVersion = vaultVersion;
        }

        public IVaultConfigurationLoader GetVaultConfigurationLoader()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new VaultConfigurationLoader();
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
