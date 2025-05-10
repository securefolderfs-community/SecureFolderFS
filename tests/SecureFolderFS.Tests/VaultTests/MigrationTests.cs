using FluentAssertions;
using NUnit.Framework;
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
            var v1VaultFolder = await MockVaultHelpers.CreateVaultV1Async();
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
            var (v2VaultFolder, _) = await MockVaultHelpers.CreateVaultV2Async();
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
    }
}
