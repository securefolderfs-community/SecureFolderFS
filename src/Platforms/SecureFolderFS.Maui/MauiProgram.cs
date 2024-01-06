using CommunityToolkit.Maui;
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
                .UseMaterialMauiIcons()
                .UseMauiCommunityToolkit()
                .UseBottomSheet();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
