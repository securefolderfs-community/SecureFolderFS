using System.IO;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;

namespace SecureFolderFS.Core.VaultLoader.VaultKeystore
{
    internal sealed class VaultKeystoreLoader : IVaultKeystoreLoader
    {
        public BaseVaultKeystore LoadVaultKeystore(Stream keystoreFileStream)
        {
            return VaultDataStore.VaultKeystore.VaultKeystore.Load(keystoreFileStream);
        }
    }
}
