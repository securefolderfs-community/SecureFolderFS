using System.IO;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultLoader.Discoverers
{
    internal sealed class FromVaultPathVaultKeystoreDiscoverer : IVaultKeystoreDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultKeystoreDiscoverer(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName)
        {
            var keystorePath = Path.Combine(vaultPath, keystoreFileName);
            return _fileOperations.OpenFile(keystorePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
