using FluentAssertions;
using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class CredsRemoveCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task CredsRemove_WithRecoveryKeyFlag_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("creds", "remove", vaultPath, "--recovery-key", "rk", "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }
}


