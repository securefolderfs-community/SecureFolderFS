using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.VaultLoader.VaultConfiguration
{
    internal sealed class VaultConfigurationLoader : IVaultConfigurationLoader
    {
        public BaseVaultConfiguration LoadVaultConfiguration(RawVaultConfiguration rawVaultConfiguration)
        {
            var baseVaultConfiguration = VaultDataStore.VaultConfiguration.VaultConfiguration.Load(rawVaultConfiguration);

            EnumAssertions.AssertCorrectContentCipherScheme(baseVaultConfiguration.ContentCipherScheme);
            EnumAssertions.AssertCorrectFileNameCipherScheme(baseVaultConfiguration.FileNameCipherScheme);

            return baseVaultConfiguration;
        }
    }
}
