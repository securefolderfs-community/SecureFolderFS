using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Extensions
{
    internal static class IocExtensions
    {
        public static IServiceCollection WithMauiServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                    .AddSingleton<IOverlayService, MauiSheetService>()
                    .AddTransient<INavigationService, MauiNavigationService>()
                    //.AddSingleton<IClipboardService, ClipboardService>()
                    //.AddSingleton<IThreadingService, ThreadingService>()
                ;
        }
    }
}
