using System.IO;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.VaultLoader.KeyDerivation;

namespace SecureFolderFS.Core.DataModels
{
    internal sealed class VaultLoadDataModel
    {
        public Stream VaultConfigurationStream { get; set; }

        public Stream VaultKeystoreStream { get; set; }

        public BaseVaultConfiguration BaseVaultConfiguration { get; set; }

        public BaseVaultKeystore BaseVaultKeystore { get; set; }

        public IMasterKeyDerivation MasterKeyDerivation { get; set; }

        public IKeyCryptor KeyCryptor { get; set; }

        public MasterKey MasterKey { get; set; }

        public IChunkFactory ChunkFactory { get; set; }

        public void Cleanup()
        {
            VaultConfigurationStream?.Dispose();
            VaultKeystoreStream?.Dispose();
            MasterKey?.Dispose();
        }
    }
}
