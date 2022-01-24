using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Security.Loader
{
    internal sealed class SecurityLoaderFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IChunkFactory _chunkFactory;

        public SecurityLoaderFactory(VaultVersion vaultVersion, IChunkFactory chunkFactory)
        {
            this._vaultVersion = vaultVersion;
            this._chunkFactory = chunkFactory;
        }

        public ISecurityLoader GetSecurityLoader()
        {
            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new SecurityLoader(_chunkFactory);
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
