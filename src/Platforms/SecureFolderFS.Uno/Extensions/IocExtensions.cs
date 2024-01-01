using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Uno.ServiceImplementation;

namespace SecureFolderFS.Uno.Extensions
{
    public static class IocExtensions
    {
        public static IServiceCollection WithUnoServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IOverlayService, UnoDialogService>()
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<IFileExplorerService, FileExplorerService>()
                .AddTransient<INavigationService, UnoNavigationService>()
                ;
        }
    }
}
