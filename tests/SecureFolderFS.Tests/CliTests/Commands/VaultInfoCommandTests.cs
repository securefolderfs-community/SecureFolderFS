using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Tests.Helpers;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class VaultInfoCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task VaultInfo_OnMissingPath_ShouldReturnVaultUnreadable()
    {
        // Arrange
        var missingPath = Path.Combine(Path.GetTempPath(), $"sffs-cli-missing-{Guid.NewGuid():N}");

        // Act
        var result = await RunCliAsync("vault", "info", missingPath, "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.VaultUnreadable);
    }

    [Test]
    public async Task VaultInfo_OnExistingVault_ShouldReturnInformation()
    {
        // Arrange
        var (folder, _) = await MockVaultHelpers.CreateVaultLatestAsync(null);

        // Act
        var result = await RunCliAsync("vault", "info", folder.Id, "--no-color");

        // TODO: Assert
        // Since Cli currently uses hardcoded SystemFolderEx, we cannot test against MemoryFolderEx
    }
}


