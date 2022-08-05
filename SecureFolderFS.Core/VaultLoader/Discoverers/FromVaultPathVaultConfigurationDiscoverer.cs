using System.IO;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultLoader.Discoverers
{
    internal sealed class FromVaultPathVaultConfigurationDiscoverer : IVaultConfigurationDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultConfigurationDiscoverer(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public Stream DiscoverVaultConfig(string vaultPath, string configFileName)
        {
            var configPath = Path.Combine(vaultPath, configFileName);
            return _fileOperations.OpenFile(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
