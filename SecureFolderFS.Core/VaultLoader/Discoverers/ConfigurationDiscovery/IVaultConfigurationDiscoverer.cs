using System.IO;

namespace SecureFolderFS.Core.VaultLoader.Discoverers.ConfigurationDiscovery
{
    /// <summary>
    /// Provides module for discovering the vault configuration file.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultConfigurationDiscoverer
    {
        /// <summary>
        /// Opens a <see cref="Stream"/> to configuration file.
        /// </summary>
        /// <param name="vaultPath">The vault path.</param>
        /// <param name="configFileName">Possible filename of the vault configuration file.</param>
        /// <returns></returns>
        Stream OpenStreamToVaultConfig(string vaultPath, string configFileName);
    }
}
