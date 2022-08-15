using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.VaultLoader.VaultKeystore
{
    internal sealed class VaultKeystoreLoaderFactory
    {
        private readonly VaultVersion _vaultVersion;

        public VaultKeystoreLoaderFactory(VaultVersion vaultVersion)
        {
            _vaultVersion = vaultVersion;
        }

        public IVaultKeystoreLoader GetVaultKeystoreLoader()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new VaultKeystoreLoader();
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
