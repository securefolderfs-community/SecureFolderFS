using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Tests.Helpers;

namespace SecureFolderFS.Tests.ServiceTests
{
    [TestClass]
    public class MigrationTests
    {
        [TestMethod]
        public async Task Create_V1Vault_MigrateTo_V2Vault_AssertSuccess()
        {
            // Arrange
            var v1VaultFolder = await MockVaultHelpers.CreateVaultV1Async();
            var service = DI.Service<IVaultManagerService>();

            // Act
            var migrator = await service.GetMigratorAsync(v1VaultFolder);
            var contract = await migrator.UnlockAsync(MockVaultHelpers.VAULT_PASSWORD);
            var result = await migrator.MigrateAsync(contract, new());

            // Assert
            Assert.IsTrue(result.Successful, result.GetMessage());
        }
    }
}
