using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ServiceImplementation.Settings;
using SecureFolderFS.Uno.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Uno.Extensions
{
    internal static class UnoIocExtensions
    {
        public static IServiceCollection WithUnoServices(this IServiceCollection serviceCollection, IModifiableFolder settingsFolder)
        {
            return serviceCollection
                    .Foundation<ISettingsService, SettingsService>(AddService.AddSingleton, _ => new(new AppSettings(settingsFolder), new UserSettings(settingsFolder)))
                    .Foundation<IMediaService, UnoMediaService>(AddService.AddSingleton)
                    .Foundation<IOverlayService, UnoDialogService>(AddService.AddSingleton)
                    .Foundation<IStorageService, UnoStorageService>(AddService.AddSingleton)
                    .Foundation<IClipboardService, UnoClipboardService>(AddService.AddSingleton)
                    .Foundation<IThreadingService, UnoThreadingService>(AddService.AddSingleton)
                    .Foundation<IFileExplorerService, UnoFileExplorerService>(AddService.AddSingleton)
                    .Foundation<INavigationService, UnoNavigationService>(AddService.AddTransient)
                ;
        }
    }
}
