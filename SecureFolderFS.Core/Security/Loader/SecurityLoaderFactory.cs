using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Security.Loader
{
    internal sealed class SecurityLoaderFactory
    {
        private readonly VaultVersion _vaultVersion;

        public SecurityLoaderFactory(VaultVersion vaultVersion)
        {
            _vaultVersion = vaultVersion;
        }

        public ISecurityLoader GetSecurityLoader()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new SecurityLoader();
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
