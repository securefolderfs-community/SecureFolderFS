using APES.UI.XF;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using The49.Maui.BottomSheet;

#if ANDROID
using Material.Components.Maui.Extensions;
using MauiIcons.Material;
#elif IOS
using MauiIcons.Cupertino;
using SecureFolderFS.Maui.Views;
#endif

namespace SecureFolderFS.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

                // Plugins
                .UseMauiCommunityToolkitMediaElement()  // https://github.com/CommunityToolkit/Maui
                .UseMauiCommunityToolkit()              // https://github.com/CommunityToolkit/Maui
                .UseBottomSheet()                       // https://github.com/the49ltd/The49.Maui.BottomSheet
                .ConfigureContextMenuContainer()        // https://github.com/anpin/ContextMenuContainer

#if ANDROID
                .UseMaterialMauiIcons()                 // https://github.com/AathifMahir/MauiIcons
                .UseMaterialComponents()                // https://github.com/mdc-maui/mdc-maui
#elif IOS
                .UseCupertinoMauiIcons()
#endif

                // Handlers
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS
                    handlers.AddHandler<ContentPageExtended, Handlers.ContentPageExHandler>();
#endif
                })

                ; // Finish initialization

            return builder.Build();
        }
    }
}
