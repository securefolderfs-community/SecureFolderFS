using CommunityToolkit.Maui;
using Material.Components.Maui.Extensions;
using MauiIcons.Material;
using Microsoft.Extensions.Logging;
using The49.Maui.BottomSheet;

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
                .UseMaterialMauiIcons()         // https://github.com/AathifMahir/MauiIcons
                .UseMauiCommunityToolkit()      // https://github.com/CommunityToolkit/Maui
                .UseMaterialComponents()        // https://github.com/mdc-maui/mdc-maui
                .UseBottomSheet()               // https://github.com/the49ltd/The49.Maui.BottomSheet
                ;

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
