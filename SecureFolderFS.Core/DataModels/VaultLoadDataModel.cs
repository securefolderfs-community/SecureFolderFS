using System.IO;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.VaultLoader.KeyDerivation;

namespace SecureFolderFS.Core.DataModels
{
    internal sealed class VaultLoadDataModel
    {
        public Stream VaultConfigurationStream { get; set; }

        public Stream VaultKeystoreStream { get; set; }

        public BaseVaultKeystore BaseVaultKeystore { get; set; }

        public IMasterKeyDerivation MasterKeyDerivation { get; set; }

        public ICipherProvider KeyCryptor { get; set; }

        public MasterKey MasterKey { get; set; }

        public IChunkFactory ChunkFactory { get; set; }

        public void Cleanup()
        {
            MasterKey?.Dispose();
        }
    }
}
