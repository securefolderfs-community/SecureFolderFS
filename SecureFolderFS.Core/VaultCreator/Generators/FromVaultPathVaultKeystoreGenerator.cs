using System.IO;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultCreator.Generators
{
    internal sealed class FromVaultPathVaultKeystoreGenerator : IVaultKeystoreDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultKeystoreGenerator(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName)
        {
            var keystorePath = Path.Combine(vaultPath, keystoreFileName);
            return _fileOperations.OpenFile(keystorePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
