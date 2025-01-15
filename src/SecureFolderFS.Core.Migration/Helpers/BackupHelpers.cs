using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Migration.Helpers
{
    internal static class BackupHelpers
    {
        public static async Task CreateConfigBackup(IModifiableFolder vaultFolder, Stream configStream, CancellationToken cancellationToken)
        {
            var backupConfigName = $"{Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME}.bkup";
            var backupConfigFile = await vaultFolder.CreateFileAsync(backupConfigName, true, cancellationToken);
            await using var backupConfigStream = await backupConfigFile.OpenWriteAsync(cancellationToken);

            await configStream.CopyToAsync(backupConfigStream, cancellationToken);
            configStream.Position = 0L;
        }

        public static async Task CreateKeystoreBackup(IModifiableFolder vaultFolder, CancellationToken cancellationToken)
        {
            var keystoreFile = await vaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);
            var backupKeystoreName = $"{Constants.Vault.Names.VAULT_KEYSTORE_FILENAME}.bkup";
            var backupKeystoreFile = await vaultFolder.CreateFileAsync(backupKeystoreName, true, cancellationToken);

            await keystoreFile.CopyContentsToAsync(backupKeystoreFile, cancellationToken);
        }
    }
}
