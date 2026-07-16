using OwlCore.Storage;
using Plugin.Maui.BottomSheet.Hosting;
using Plugin.Maui.BottomSheet.Navigation;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.ServiceImplementation.Settings;
using SecureFolderFS.Maui.Sheets;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;
#if APP_PLATFORM_PRESENT
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.AppPlatform.Services;
#endif

namespace SecureFolderFS.Maui.Extensions
{
    internal static class IocExtensions
    {
        public static IServiceCollection WithMauiServices(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                    .Foundation<ISettingsService, SettingsService>(AddService.AddSingleton, _ => new(new MauiAppSettings(settingsFolder), new UserSettings(settingsFolder)))
                    .Foundation<IOverlayService, MauiOverlayService>(AddService.AddSingleton)
                    .Foundation<IAccountService, MauiAccountService>(AddService.AddSingleton)
                    .Foundation<IClipboardService, MauiClipboardService>(AddService.AddSingleton)
                    .Foundation<IThreadingService, MauiThreadingService>(AddService.AddSingleton)
                    .Foundation<IPropertyStoreService, MauiPropertyStoreService>(AddService.AddSingleton)
                    .Foundation<IBottomSheetNavigationService, BottomSheetNavigationService>(AddService.AddSingleton)
                    .Foundation<INavigationService, MauiNavigationService>(AddService.AddTransient)

#if APP_PLATFORM_PRESENT
                    .Foundation<IDeviceKeyStore, SecurePropertyKeyStore>(AddService.AddSingleton, sp => new SecurePropertyKeyStore(sp.GetRequiredService<IPropertyStoreService>().SecurePropertyStore, settingsFolder))
                    .Foundation<IAccountProvider, AppPlatformAccountProvider>(AddService.AddSingleton, sp => new AppPlatformAccountProvider(sp.GetRequiredService<IDeviceKeyStore>()))
#endif

                    .AddBottomSheet<ViewOptionsSheet>(nameof(ViewOptionsSheet))
                ;
        }
    }
}
