using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Tests.Helpers;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.Tests.VaultTests
{
    [TestFixture]
    public class MigrationTests
    {
        [Test]
        public async Task Create_V1Vault_MigrateTo_V2Vault_NoThrow()
        {
            // Arrange
            var v1VaultFolder = await MockVaultHelpers.CreateVaultV1Async(null);
            var service = DI.Service<IVaultService>();

            // Act
            var migrator = await service.GetMigratorAsync(v1VaultFolder);
            var contract = await migrator.UnlockAsync(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));
            await migrator.MigrateAsync(contract, new());

            // Assert
            var v2ConfigFile = await v1VaultFolder.GetFileByNameAsync(Names.VAULT_CONFIGURATION_FILENAME);
            var text = await v2ConfigFile.ReadAllTextAsync();

            text.Should()
                .Contain(Associations.ASSOC_VAULT_ID).And
                .Contain(Associations.ASSOC_AUTHENTICATION).And
                .Contain("\"version\": 2");
        }

        [Test]
        public async Task Create_V2Vault_MigrateTo_V3Vault_NoThrow()
        {
            // Arrange
            var (v2VaultFolder, _) = await MockVaultHelpers.CreateVaultV2Async(null);
            var service = DI.Service<IVaultService>();

            // Act
            var migrator = await service.GetMigratorAsync(v2VaultFolder);
            var keySequence = new KeySequence();
            keySequence.Add(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));

            var contract = await migrator.UnlockAsync(keySequence);
            await migrator.MigrateAsync(contract, new());

            // Assert
            var v3ConfigFile = await v2VaultFolder.GetFileByNameAsync(Names.VAULT_CONFIGURATION_FILENAME);
            var text = await v3ConfigFile.ReadAllTextAsync();

            text.Should()
                .Contain(Associations.ASSOC_RECYCLE_SIZE).And
                .Contain(Associations.ASSOC_FILENAME_ENCODING_ID).And
                .Contain("\"version\": 3");
        }

        [Test]
        public async Task Create_V3Vault_MigrateTo_V4Vault_NoThrow()
        {
            // Arrange
            var (v3VaultFolder, _) = await MockVaultHelpers.CreateVaultV3Async(null);
            var service = DI.Service<IVaultService>();

            // Act
            using var migrator = await service.GetMigratorAsync(v3VaultFolder);
            using var keySequence = new KeySequence();
            keySequence.Add(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));

            var contract = await migrator.UnlockAsync(keySequence);
            await migrator.MigrateAsync(contract, new());

            // Assert
            var v4ConfigFile = await v3VaultFolder.GetFileByNameAsync(Names.VAULT_CONFIGURATION_FILENAME);
            var text = await v4ConfigFile.ReadAllTextAsync();

            text.Should()
                .Contain(Associations.ASSOC_FILENAME_SHORTENING).And
                .Contain(Associations.ASSOC_FILENAME_ENCODING_ID).And
                .Contain("\"version\": 4");
        }

        [Test]
        public async Task Create_V3Vault_MigrateTo_V4Vault_UnlocksWithSameCredentials()
        {
            // Arrange
            var (v3VaultFolder, _) = await MockVaultHelpers.CreateVaultV3Async(null);
            var vaultService = DI.Service<IVaultService>();
            var vaultManagerService = DI.Service<IVaultManagerService>();

            using (var migrator = await vaultService.GetMigratorAsync(v3VaultFolder))
            {
                using var keySequence = new KeySequence();
                keySequence.Add(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));

                var contract = await migrator.UnlockAsync(keySequence);
                await migrator.MigrateAsync(contract, new());
            }

            // Act
            // The keystore is carried over untouched by the migration, so the original password must still open
            // the vault. This also exercises the V4 payload MAC, which the unlock routine validates
            using var password = new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD);
            var unlockContract = await vaultManagerService.UnlockAsync(v3VaultFolder, password);

            // Assert
            unlockContract.Should().NotBeNull();

            var vaultOptions = await vaultService.GetVaultOptionsAsync(v3VaultFolder);
            vaultOptions.Version.Should().Be(Versions.V4);
            vaultOptions.ShorteningThreshold.Should().Be(0);
            vaultOptions.ContentCipherId.Should().Be(Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305);
            vaultOptions.FileNameCipherId.Should().Be(Core.Cryptography.Constants.CipherId.AES_SIV);
            vaultOptions.NameEncodingId.Should().Be(Core.Cryptography.Constants.CipherId.ENCODING_BASE4K);
            vaultOptions.VaultId.Should().Be("3a169788-6149-4583-ad92-f68113e70e23");

            unlockContract.Dispose();
        }

        [Test]
        public async Task Create_V3Vault_MigrateTo_V4Vault_UsingRecoveryKey_NoThrow()
        {
            // Arrange
            var (v3VaultFolder, recoveryKey) = await MockVaultHelpers.CreateVaultV3Async(null);
            var vaultService = DI.Service<IVaultService>();
            var vaultManagerService = DI.Service<IVaultManagerService>();

            // Act
            using (var migrator = await vaultService.GetMigratorAsync(v3VaultFolder))
            {
                var contract = await migrator.RecoverAsync(recoveryKey);
                await migrator.MigrateAsync(contract, new());
            }

            // Assert
            // Recovering does not re-key the vault, so the configured password keeps working afterwards
            using var password = new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD);
            var unlockContract = await vaultManagerService.UnlockAsync(v3VaultFolder, password);

            unlockContract.Should().NotBeNull();
            unlockContract.Dispose();
        }

        [Test]
        public async Task Create_V3Vault_WithTamperedConfiguration_MigrateTo_V4Vault_Throws()
        {
            // Arrange
            var (v3VaultFolder, _) = await MockVaultHelpers.CreateVaultV3Async(null);
            var service = DI.Service<IVaultService>();

            // Downgrade file name encryption without updating the (unforgeable) payload MAC
            var configFile = await v3VaultFolder.GetFileByNameAsync(Names.VAULT_CONFIGURATION_FILENAME);
            var configText = await configFile.ReadAllTextAsync();
            await configFile.WriteAllTextAsync(configText.Replace("\"filenameCipherScheme\": \"AES-SIV\"", "\"filenameCipherScheme\": \"\""));

            // Act
            using var migrator = await service.GetMigratorAsync(v3VaultFolder);
            using var keySequence = new KeySequence();
            keySequence.Add(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));

            // Assert
            // Migrating re-signs the configuration with the real MAC key, so a tampered configuration must
            // be rejected instead of being turned into a validly signed V4 one
            var unlock = async () => await migrator.UnlockAsync(keySequence);
            await unlock.Should().ThrowAsync<CryptographicException>();
        }
    }
}
