using System.IO;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;

namespace SecureFolderFS.Core.DataModels
{
    internal sealed class VaultCreationDataModel
    {
        public VaultPath VaultPath { get; set; }

        public IKeyCryptor KeyCryptor { get; set; }

        public IFileOperations FileOperations { get; set; }

        public IDirectoryOperations DirectoryOperations { get; set; }

        public Stream VaultKeystoreStream { get; set; }

        public Stream VaultConfigurationStream { get; set; }

        public BaseVaultKeystore BaseVaultKeystore { get; set; }

        public SecretKey MacKey { get; set; }

        public ContentCipherScheme ContentCipherScheme { get; set; }

        public FileNameCipherScheme FileNameCipherScheme { get; set; }

        public void Cleanup()
        {
            KeyCryptor?.Dispose();
            VaultKeystoreStream?.Dispose();
            VaultConfigurationStream?.Dispose();
            MacKey?.Dispose();
        }
    }
}
