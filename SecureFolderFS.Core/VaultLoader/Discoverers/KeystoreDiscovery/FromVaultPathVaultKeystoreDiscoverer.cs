using System.IO;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultLoader.Discoverers.KeystoreDiscovery
{
    internal sealed class FromVaultPathVaultKeystoreDiscoverer : IVaultKeystoreDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultKeystoreDiscoverer(IFileOperations fileOperations)
        {
            this._fileOperations = fileOperations;
        }

        public Stream OpenStreamToVaultKeystore(string vaultPath, string keystoreFileName)
        {
            string keystorePath = Path.Combine(vaultPath, keystoreFileName);

            return _fileOperations.OpenFile(keystorePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
