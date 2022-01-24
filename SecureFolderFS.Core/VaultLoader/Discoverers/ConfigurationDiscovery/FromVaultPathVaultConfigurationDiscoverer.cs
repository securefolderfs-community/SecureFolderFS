using System.IO;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultLoader.Discoverers.ConfigurationDiscovery
{
    internal sealed class FromVaultPathVaultConfigurationDiscoverer : IVaultConfigurationDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultConfigurationDiscoverer(IFileOperations fileOperations)
        {
            this._fileOperations = fileOperations;
        }

        public Stream OpenStreamToVaultConfig(string vaultPath, string configFileName)
        {
            string configPath = Path.Combine(vaultPath, configFileName);

            return _fileOperations.OpenFile(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
