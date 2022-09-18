using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using System;
using System.IO;

namespace SecureFolderFS.Core.DataModels
{
    [Obsolete]
    internal sealed class VaultCreationDataModel
    {
        public VaultPath VaultPath { get; set; }

        public ICipherProvider KeyCryptor { get; set; }

        public Stream VaultKeystoreStream { get; set; }

        public Stream VaultConfigurationStream { get; set; }

        public BaseVaultKeystore BaseVaultKeystore { get; set; }

        public SecretKey MacKey { get; set; }

        public ContentCipherScheme ContentCipherScheme { get; set; }

        public FileNameCipherScheme FileNameCipherScheme { get; set; }

        public void Cleanup()
        {
            MacKey?.Dispose();
        }
    }
}
