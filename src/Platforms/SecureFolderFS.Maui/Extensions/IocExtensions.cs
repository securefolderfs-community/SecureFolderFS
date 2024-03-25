using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Extensions
{
    internal static class IocExtensions
    {
        public static IServiceCollection WithMauiServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                    .AddSingleton<IOverlayService, SheetService>()
                    //.AddSingleton<IClipboardService, ClipboardService>()
                    //.AddSingleton<IThreadingService, ThreadingService>()
                    .AddSingleton<IStorageService, MauiStorageService>()
                    .AddSingleton<IFileExplorerService, MauiFileExplorerService>()
                    //.AddTransient<INavigationService, UnoNavigationService>()
                ;
        }
    }
}
