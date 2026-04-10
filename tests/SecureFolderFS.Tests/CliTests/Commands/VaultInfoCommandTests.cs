using FluentAssertions;
using NUnit.Framework;

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
}


