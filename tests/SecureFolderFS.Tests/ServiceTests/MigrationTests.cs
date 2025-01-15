using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Tests.Helpers;

namespace SecureFolderFS.Tests.ServiceTests
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
            var v2ConfigFile = await v1VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME);
            await using var v2ConfigStream = await v2ConfigFile.OpenReadAsync();

            v2ConfigStream.Length.Should().BeGreaterThan(0L);
        }
    }
}
