using OwlCore.Storage;
using Plugin.Maui.BottomSheet.Hosting;
using Plugin.Maui.BottomSheet.Navigation;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.ServiceImplementation.Settings;
using SecureFolderFS.Maui.Sheets;
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
                    .AddSingleton<IShareService, MauiShareService>()
                    .AddSingleton<IOverlayService, MauiOverlayService>()
                    .AddSingleton<IClipboardService, MauiClipboardService>()
                    .AddSingleton<IBottomSheetNavigationService, BottomSheetNavigationService>()
                    .AddTransient<INavigationService, MauiNavigationService>()
                    .AddBottomSheet<ViewOptionsSheet>(nameof(ViewOptionsSheet))
                ;
        }
    }
}
