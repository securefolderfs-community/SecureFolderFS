using System;
using System.IO;

namespace SecureFolderFS.Core.Discoverers
{
    /// <summary>
    /// Provides module for discovering the vault configuration file.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    [Obsolete]
    public interface IVaultConfigurationDiscoverer
    {
        /// <summary>
        /// Opens a <see cref="Stream"/> to configuration file.
        /// </summary>
        /// <param name="vaultPath">The vault path.</param>
        /// <param name="configFileName">Possible filename of the vault configuration file.</param>
        /// <returns></returns>
        Stream DiscoverVaultConfig(string vaultPath, string configFileName);
    }
}
