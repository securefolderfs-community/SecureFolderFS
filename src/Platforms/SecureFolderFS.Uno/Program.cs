#if WINDOWS
using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT;

namespace SecureFolderFS.Uno
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ComWrappersSupport.InitializeComWrappers();

            var currentInstance = AppInstance.GetCurrent();
            currentInstance.Activated += OnActivated;

            Application.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                _ = new App();
            });
        }

        private static async void OnActivated(object? sender, AppActivationArguments args)
        {
            if (App.Instance is not null)
                await App.Instance.OnActivatedAsync(args);
        }
    }
}
#endif
