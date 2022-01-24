using System.IO;

namespace SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration
{
    /// <summary>
    /// Provides module for generating the vault configuration file.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultConfigurationGenerator
    {
        /// <summary>
        /// Generates configuration file and opens a <see cref="Stream"/> to it.
        /// </summary>
        /// <param name="vaultPath">The vault path.</param>
        /// <param name="configFileName">Possible filename of the vault configuration file.</param>
        /// <returns></returns>
        Stream GenerateVaultConfig(string vaultPath, string configFileName);
    }
}
