using SecureFolderFS.Core.Discoverers;
using System.IO;

namespace SecureFolderFS.Core.VaultCreator.Generators
{
    internal sealed class FromVaultPathVaultConfigurationGenerator : IVaultConfigurationDiscoverer
    {
        public Stream DiscoverVaultConfig(string vaultPath, string configFileName)
        {
            var configPath = Path.Combine(vaultPath, configFileName);
            return File.Open(configPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
