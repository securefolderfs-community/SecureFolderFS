using FluentAssertions;
using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class CredsChangeCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task CredsChange_WithRecoveryKeyFlag_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("creds", "change", vaultPath, "--recovery-key", "rk", "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }
}


