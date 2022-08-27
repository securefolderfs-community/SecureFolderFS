using SecureFolderFS.Core.Discoverers;
using System.IO;

namespace SecureFolderFS.Core.VaultCreator.Generators
{
    internal sealed class FromVaultPathVaultKeystoreGenerator : IVaultKeystoreDiscoverer
    {
        public Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName)
        {
            var keystorePath = Path.Combine(vaultPath, keystoreFileName);
            return File.Open(keystorePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
