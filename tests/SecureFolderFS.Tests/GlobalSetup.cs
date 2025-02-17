using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Tests.ServiceImplementation;
using SecureFolderFS.UI;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public static void GlobalInitialize()
        {
            var settingsFolderPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new MemoryFolder(settingsFolderPath, Path.GetFileName(settingsFolderPath));

            var serviceProvider = BuildServiceProvider(settingsFolder);
            DI.Default.SetServiceProvider(serviceProvider);
        }

        private static IServiceProvider BuildServiceProvider(IModifiableFolder settingsFolder)
        {
            return new ServiceCollection()

                // Singleton services
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                .AddSingleton<IChangelogService, GitHubChangelogService>()
                .AddSingleton<IVaultService, VaultService>()
                //.AddSingleton<IApplicationService, MockApplicationService>()
                .AddSingleton<IVaultManagerService, VaultManagerService>()
                .AddSingleton<IRecycleBinService, RecycleBinService>()
                .AddSingleton<IVaultFileSystemService, MockVaultFileSystemService>()
                .AddSingleton<IVaultCredentialsService, MockVaultCredentialsService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()

                .BuildServiceProvider();
        }
    }
}
