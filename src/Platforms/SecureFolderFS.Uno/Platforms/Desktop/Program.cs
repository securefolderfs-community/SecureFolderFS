using System;
using Uno.UI.Hosting;

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

            var appHost = UnoPlatformHostBuilder.Create()
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
