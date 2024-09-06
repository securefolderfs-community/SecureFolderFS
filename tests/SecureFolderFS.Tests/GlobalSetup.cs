using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Tests.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Tests
{
    [TestClass]
    public class GlobalSetup
    {
        [AssemblyInitialize]
        public static void GlobalInitialize(TestContext testContext)
        {
            var settingsFolderPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
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
                .AddSingleton<IVaultService, MockVaultService>()
                //.AddSingleton<IApplicationService, MockApplicationService>()
                .AddSingleton<IVaultManagerService, MockVaultManagerService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()

                .BuildServiceProvider();
        }
    }
}
