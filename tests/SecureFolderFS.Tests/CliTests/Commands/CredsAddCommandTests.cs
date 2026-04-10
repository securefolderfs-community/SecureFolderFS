using FluentAssertions;
using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class CredsAddCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task CredsAdd_WithRecoveryKeyFlag_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("creds", "add", vaultPath, "--recovery-key", "rk", "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }
}


