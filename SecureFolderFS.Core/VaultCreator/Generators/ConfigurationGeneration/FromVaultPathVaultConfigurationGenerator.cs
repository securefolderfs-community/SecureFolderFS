using System.IO;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration
{
    internal sealed class FromVaultPathVaultConfigurationGenerator : IVaultConfigurationGenerator
    {
        private readonly IFileOperations _fileOperations;

        public FromVaultPathVaultConfigurationGenerator(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public Stream GenerateVaultConfig(string vaultPath, string configFileName)
        {
            string configPath = Path.Combine(vaultPath, configFileName);

            return _fileOperations.OpenFile(configPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }
    }
}
