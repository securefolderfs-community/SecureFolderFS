using System.IO;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultCreator.Generators
{
    internal sealed class FromVaultPathVaultConfigurationGenerator : IVaultConfigurationDiscoverer
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultConfigurationGenerator(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public Stream DiscoverVaultConfig(string vaultPath, string configFileName)
        {
            var configPath = Path.Combine(vaultPath, configFileName);
            return _fileOperations.OpenFile(configPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
