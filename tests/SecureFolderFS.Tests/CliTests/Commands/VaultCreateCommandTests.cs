using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Core;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class VaultCreateCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task VaultCreate_WithoutCredential_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("vault", "create", vaultPath, "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }

    [Test]
    public async Task VaultCreate_WithPasswordEnvironmentVariable_ShouldCreateVault()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();
        var environmentVariables = new Dictionary<string, string?>
        {
            ["SFFS_PASSWORD"] = "Password#1"
        };

        // Act
        var result = await RunCliAsync(["vault", "create", vaultPath, "--no-color"], environmentVariables);

        // Assert
        var configPath = Path.Combine(vaultPath, Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME);

        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.Success);
        File.Exists(configPath).Should().BeTrue();
    }
}


