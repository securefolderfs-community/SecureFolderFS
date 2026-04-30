using FluentAssertions;
using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class VaultRunCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task VaultRun_WithoutReadOrWrite_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("vault", "run", vaultPath, "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }
}


