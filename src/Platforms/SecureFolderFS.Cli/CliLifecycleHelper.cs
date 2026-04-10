using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Cli;

internal sealed class CliLifecycleHelper
{
    public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    public IServiceProvider BuildServiceProvider(bool quiet)
    {
        var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
        var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
        EnsureSettingsFile(settingsFolderPath, SecureFolderFS.UI.Constants.FileNames.APPLICATION_SETTINGS_FILENAME);
        EnsureSettingsFile(settingsFolderPath, SecureFolderFS.UI.Constants.FileNames.USER_SETTINGS_FILENAME);

        ServiceCollection
            .Foundation<IVaultService, VaultService>(AddService.AddSingleton)
            .Foundation<IVaultManagerService, VaultManagerService>(AddService.AddSingleton)
            .Foundation<IVaultFileSystemService, CliVaultFileSystemService>(AddService.AddSingleton)
            .Foundation<IVaultCredentialsService, CliVaultCredentialsService>(AddService.AddSingleton)
            .Foundation<ITelemetryService, DebugTelemetryService>(AddService.AddSingleton)
            .Foundation<ISettingsService, SettingsService>(AddService.AddSingleton,
                _ => new SettingsService(new AppSettings(settingsFolder), new UserSettings(settingsFolder)));

        ServiceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(quiet ? LogLevel.Error : LogLevel.Information);
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        });

        var serviceProvider = ServiceCollection.BuildServiceProvider();
        DI.Default.SetServiceProvider(serviceProvider);
        return serviceProvider;
    }

    private static void EnsureSettingsFile(string settingsFolderPath, string fileName)
    {
        var filePath = Path.Combine(settingsFolderPath, fileName);
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "{}");
            return;
        }

        var info = new FileInfo(filePath);
        if (info.Length == 0)
            File.WriteAllText(filePath, "{}");
    }
}


