using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Storage.MemoryStorageEx;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock password
        public const string VAULT_PASSWORD = "t";

        private static async Task<IFolder> SetupMockVault(string configString, string keystoreString, CancellationToken cancellationToken)
        {
            var vaultPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), "TestVault");
            var vaultFolder = new MemoryFolderEx(vaultPath, Path.GetFileName(vaultPath));

            // Create necessary vault configuration
            var configFile = await vaultFolder.CreateFileAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, false, cancellationToken);
            var keystoreFile = await vaultFolder.CreateFileAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, false, cancellationToken);
            var contentFolder = await vaultFolder.CreateFolderAsync("content", false, cancellationToken);

            await configFile.WriteTextAsync(configString, cancellationToken);
            await keystoreFile.WriteTextAsync(keystoreString, cancellationToken);

            _ = contentFolder;
            return vaultFolder;
        }
    }
}
