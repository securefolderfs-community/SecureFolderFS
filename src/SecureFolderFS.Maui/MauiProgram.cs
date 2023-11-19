using CommunityToolkit.Mvvm.DependencyInjection;
using MauiIcons.SegoeFluent;
using Microsoft.Extensions.Logging;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Storage.NativeStorage;

namespace SecureFolderFS.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseSegoeFluentMauiIcons();

            ConfigureServices(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            Ioc.Default.ConfigureServices(app.Services);

            return app;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
#if UNPACKAGED
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.LocalSettings.SETTINGS_FOLDER_NAME);
#else
            var settingsFolderPath = Path.Combine(FileSystem.AppDataDirectory, SecureFolderFS.UI.Constants.LocalSettings.SETTINGS_FOLDER_NAME);
#endif

            var settingsFolder = new NativeFolder(Directory.CreateDirectory(settingsFolderPath));

            // Singleton services
            services

                // Singleton services
                .AddSingleton<ISettingsService, SettingsService>(_ => new(settingsFolder))
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                .AddSingleton<IVaultService, VaultService>()
                //.AddSingleton<IDialogService, DialogService>()
                //.AddSingleton<IPrinterService, WindowsPrinterService>()
                //.AddSingleton<IClipboardService, ClipboardService>()
                //.AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<IStorageService, NativeStorageService>()
                //.AddSingleton<IApplicationService, ApplicationService>()
                //.AddSingleton<IFileExplorerService, FileExplorerService>()
                .AddSingleton<IChangelogService, GitHubChangelogService>()

                // Transient services
                //.AddTransient<INavigationService, WindowsNavigationService>()

                // ILocalizationService
#if UNPACKAGED
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
#else
                //.AddSingleton<ILocalizationService, PackageLocalizationService>()
#endif

                // IIApService, IUpdateService
#if DEBUG || UNPACKAGED
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
#else
                .AddSingleton<IIapService, DebugIapService>() // .AddSingleton<IIapService, MicrosoftStoreIapService>() // TODO: Change in the future
                .AddSingleton<IUpdateService, MicrosoftStoreUpdateService>()
#endif

                // ITelemetryService
#if DEBUG
                //.AddSingleton<ITelemetryService, AppCenterTelemetryService>()
#else
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#endif

                ; // Finish service initialization
        }
    }
}
