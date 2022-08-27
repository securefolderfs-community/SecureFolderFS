using System.IO;
using SecureFolderFS.Core.Discoverers;

namespace SecureFolderFS.Core.VaultLoader.Discoverers
{
    internal sealed class FromVaultPathVaultKeystoreDiscoverer : IVaultKeystoreDiscoverer
    {
        public Stream DiscoverVaultKeystore(string vaultPath, string keystoreFileName)
        {
            var keystorePath = Path.Combine(vaultPath, keystoreFileName);
            return File.Open(keystorePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
