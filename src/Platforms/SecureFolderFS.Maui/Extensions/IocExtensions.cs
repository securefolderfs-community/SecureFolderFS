using OwlCore.Storage;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.ServiceImplementation.Settings;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;

namespace SecureFolderFS.Maui.Extensions
{
    internal static class IocExtensions
    {
        public static IServiceCollection WithMauiServices(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                    .AddSingleton<ISettingsService, SettingsService>(_ => new(new MauiAppSettings(settingsFolder), new UserSettings(settingsFolder)))
                    .AddSingleton<IMediaService, MauiMediaService>()
                    .AddSingleton<IOverlayService, MauiOverlayService>()
                    .AddTransient<INavigationService, MauiNavigationService>()
                    .AddSingleton<IClipboardService, MauiClipboardService>()
                    //.AddSingleton<IThreadingService, ThreadingService>()
                ;
        }
    }
}
