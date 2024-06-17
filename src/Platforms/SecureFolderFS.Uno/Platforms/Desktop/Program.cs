using System;
using Uno.UI.Runtime.Skia;

namespace SecureFolderFS.Uno
{
    public sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
#if (!useDependencyInjection && useLoggingFallback)
        App.InitializeLogging();
#endif

            var appHost = SkiaHostBuilder.Create()
                .App(() => new App())
                .UseX11()
                .UseLinuxFrameBuffer()
                .UseMacOS()
                .UseWindows()
                .Build();

            appHost.Run();
        }
    }
}
