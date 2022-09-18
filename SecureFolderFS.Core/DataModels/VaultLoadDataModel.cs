using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.VaultLoader.KeyDerivation;
using System;
using System.IO;

namespace SecureFolderFS.Core.DataModels
{
    [Obsolete]
    internal sealed class VaultLoadDataModel
    {
        public Stream VaultConfigurationStream { get; set; }

        public Stream VaultKeystoreStream { get; set; }

        public BaseVaultKeystore BaseVaultKeystore { get; set; }

        public IMasterKeyDerivation MasterKeyDerivation { get; set; }

        public ICipherProvider KeyCryptor { get; set; }

        public MasterKey MasterKey { get; set; }

        public void Cleanup()
        {
            MasterKey?.Dispose();
        }
    }
}
