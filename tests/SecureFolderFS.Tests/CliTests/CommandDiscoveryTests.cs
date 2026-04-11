using CliFx;
using CliFx.Attributes;
using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Cli.Commands;
using System.Reflection;

namespace SecureFolderFS.Tests.CliTests;

[TestFixture]
public class CommandDiscoveryTests
{
    [Test]
    public void CliAssembly_ExposesExpectedCommandTypes()
    {
        // Arrange
        var assembly = typeof(VaultCreateCommand).Assembly;

        // Act
        var commandTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                typeof(ICommand).IsAssignableFrom(type) &&
                type.GetCustomAttribute<CommandAttribute>() is not null)
            .Select(type => type.Name)
            .ToArray();

        // Assert
        commandTypes.Should().BeEquivalentTo(
        [
            nameof(CredsAddCommand),
            nameof(CredsChangeCommand),
            nameof(CredsRemoveCommand),
            nameof(VaultCreateCommand),
            nameof(VaultInfoCommand),
            nameof(VaultMountCommand),
            nameof(VaultRunCommand),
            nameof(VaultShellCommand),
            nameof(VaultUnmountCommand)
        ]);
    }
}

