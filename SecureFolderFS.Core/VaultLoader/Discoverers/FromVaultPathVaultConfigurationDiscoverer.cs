using SecureFolderFS.Core.Discoverers;
using System.IO;

namespace SecureFolderFS.Core.VaultLoader.Discoverers
{
    internal sealed class FromVaultPathVaultConfigurationDiscoverer : IVaultConfigurationDiscoverer
    {
        public Stream DiscoverVaultConfig(string vaultPath, string configFileName)
        {
            var configPath = Path.Combine(vaultPath, configFileName);
            return File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
