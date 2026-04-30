using FluentAssertions;
using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests.Commands;

[TestFixture]
public class VaultShellCommandTests : BaseCliCommandTests
{
    [Test]
    public async Task VaultShell_WithoutAnyCredential_ShouldReturnBadArguments()
    {
        // Arrange
        var vaultPath = CreateTempDirectory();

        // Act
        var result = await RunCliAsync("vault", "shell", vaultPath, "--no-color");

        // Assert
        result.ProcessExitCode.Should().Be(CliExpectedExitCodes.BadArguments);
    }
}


