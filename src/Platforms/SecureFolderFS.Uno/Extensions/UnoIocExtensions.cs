using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;
using SecureFolderFS.Uno.ServiceImplementation;

namespace SecureFolderFS.Uno.Extensions
{
    internal static class UnoIocExtensions
    {
        public static IServiceCollection WithUnoServices(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                .AddSingleton<ISettingsService, SettingsService>(_ => new(new AppSettings(settingsFolder), new UserSettings(settingsFolder)))
                .AddSingleton<IImageService, UnoImageService>()
                .AddSingleton<IOverlayService, UnoDialogService>()
                .AddSingleton<IStorageService, UnoStorageService>()
                .AddSingleton<IClipboardService, UnoClipboardService>()
                .AddSingleton<IThreadingService, UnoThreadingService>()
                .AddSingleton<IFileExplorerService, UnoFileExplorerService>()
                .AddTransient<INavigationService, UnoNavigationService>()
                ;
        }
    }
}
