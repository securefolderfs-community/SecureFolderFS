using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.VaultLoader.VaultConfiguration
{
    /// <summary>
    /// Provides module for loading vault configuration.
    /// </summary>
    internal interface IVaultConfigurationLoader
    {
        /// <summary>
        /// Loads <see cref="BaseVaultConfiguration"/> from provided <paramref name="rawVaultConfiguration"/>.
        /// </summary>
        /// <param name="rawVaultConfiguration">The data extracted from the configuration file.</param>
        /// <returns></returns>
        BaseVaultConfiguration LoadVaultConfiguration(RawVaultConfiguration rawVaultConfiguration);
    }
}
