using CliFx;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Cli;
using SecureFolderFS.Cli.Commands;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Tests.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Tests.CliTests;

internal static class CliTestHost
{
    public static async Task<CliExecutionResult> RunAsync(string[] args, string? standardInput = null, IReadOnlyDictionary<string, string?>? environmentVariables = null)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var originalIn = Console.In;

        var outputWriter = new StringWriter();
        var errorWriter = new StringWriter();

        var originalEnvironment = new Dictionary<string, string?>();
        if (environmentVariables is not null)
        {
            foreach (var pair in environmentVariables)
            {
                originalEnvironment[pair.Key] = Environment.GetEnvironmentVariable(pair.Key);
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        var settingsPath = Path.Combine(Path.GetTempPath(), $"sffs-cli-tests-settings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(settingsPath);

        var services = BuildServiceProvider(settingsPath);

        try
        {
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);
            Console.SetIn(new StringReader(standardInput ?? string.Empty));

            Environment.ExitCode = 0;
            var app = BuildApplication(services);
            var appExitCode = await app.RunAsync(args);

            return new CliExecutionResult(
                appExitCode,
                Environment.ExitCode,
                outputWriter.ToString(),
                errorWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
            Console.SetIn(originalIn);

            // Re-initialize the shared test DI root because CLI tests replace DI.Default.
            GlobalSetup.GlobalInitialize();

            foreach (var pair in originalEnvironment)
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);

            try
            {
                Directory.Delete(settingsPath, recursive: true);
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }

    private static IServiceProvider BuildServiceProvider(string settingsPath)
    {
        var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsPath));

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
            .AddSingleton<IChangelogService, GitHubChangelogService>()
            .AddSingleton<IVaultService, VaultService>()
            .AddSingleton<IFileExplorerService, MockFileExplorerService>()
            .AddSingleton<IVaultManagerService, VaultManagerService>()
            .AddSingleton<IRecycleBinService, RecycleBinService>()
            .AddSingleton<IVaultFileSystemService, MockVaultFileSystemService>()
            .AddSingleton<IVaultCredentialsService, MockVaultCredentialsService>()
            .AddSingleton<ITelemetryService, DebugTelemetryService>()
            .AddSingleton<IIapService, DebugIapService>()
            .AddSingleton<IUpdateService, DebugUpdateService>()
            .AddSingleton<ILocalizationService, MockLocalizationService>()
            .AddSingleton<CredentialReader>()
            .BuildServiceProvider();

        DI.Default.SetServiceProvider(serviceProvider);
        return serviceProvider;
    }

    private static CliApplication BuildApplication(IServiceProvider services)
    {
        return new CliApplicationBuilder()
            .AddCommand<CredsAddCommand>()
            .AddCommand<CredsChangeCommand>()
            .AddCommand<CredsRemoveCommand>()
            .AddCommand<VaultCreateCommand>()
            .AddCommand<VaultInfoCommand>()
            .AddCommand<VaultMountCommand>()
            .AddCommand<VaultRunCommand>()
            .AddCommand<VaultShellCommand>()
            .AddCommand<VaultUnmountCommand>()
            .UseTypeActivator(type => ActivatorUtilities.CreateInstance(services, type))
            .Build();
    }
}

