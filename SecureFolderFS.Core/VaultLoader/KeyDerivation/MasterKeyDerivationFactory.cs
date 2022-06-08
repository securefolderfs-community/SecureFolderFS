using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.VaultLoader.KeyDerivation
{
    internal sealed class MasterKeyDerivationFactory
    {
        private readonly VaultVersion _vaultVersion;

        public MasterKeyDerivationFactory(VaultVersion vaultVersion)
        {
            _vaultVersion = vaultVersion;
        }

        public IMasterKeyDerivation GetMasterKeyDerivation()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new MasterKeyDerivation();
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
