using APES.UI.XF;
using CommunityToolkit.Maui;
using LibVLCSharp.MAUI;
using Plugin.Maui.BottomSheet.Hosting;
using Xe.AcrylicView;
#if ANDROID
using MauiIcons.Material;
#elif IOS
using MauiIcons.Cupertino;
using SecureFolderFS.Maui.Handlers;
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
                .UseBottomSheet()                       // https://github.com/lucacivale/Maui.BottomSheet
                .ConfigureContextMenuContainer()        // https://github.com/anpin/ContextMenuContainer
                .UseLibVLCSharp()                       // https://github.com/videolan/libvlcsharp
                .UseAcrylicView()                       // https://github.com/sswi/AcrylicView.MAUI

#if ANDROID
                .UseMaterialMauiIcons()                 // https://github.com/AathifMahir/MauiIcons
#elif IOS
                .UseCupertinoMauiIcons()
#endif

                // Handlers
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS
                    //handlers.AddHandler<ContentPage, Handlers.ContentPageHandler>();
                    handlers.AddHandler<Slider, CustomSliderHandler>();
                    handlers.AddHandler<ContentPageExtended, ContentPageExHandler>();
#endif
                })

                ; // Finish initialization

            return builder.Build();
        }
    }
}
