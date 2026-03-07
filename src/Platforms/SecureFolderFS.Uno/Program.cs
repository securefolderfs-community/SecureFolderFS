#if WINDOWS
using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT;

namespace SecureFolderFS.Uno
{
    public static class Program
    {
        /// <summary>
        /// Stores activation arguments from initial launch to be processed after app initialization.
        /// </summary>
        public static AppActivationArguments? InitialActivationArgs { get; private set; }

        [STAThread]
        private static void Main(string[] args)
        {
            ComWrappersSupport.InitializeComWrappers();

            // Get the current activation arguments
            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            // Try to register as the main instance
            var mainInstance = AppInstance.FindOrRegisterForKey(UI.Constants.MAIN_INSTANCE_ID);

            if (!mainInstance.IsCurrent)
            {
                // Another instance is already running, redirect activation to it
                RedirectActivationTo(mainInstance, activationArgs);
                return;
            }

            // This is the main instance - register for subsequent activations
            mainInstance.Activated += OnActivated;

            // Store the initial activation arguments for processing after app initialization
            // This handles the case when app is launched via file activation
            if (activationArgs.Kind != ExtendedActivationKind.Launch)
            {
                InitialActivationArgs = activationArgs;
            }

            Application.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                _ = new App();
            });
        }

        private static void RedirectActivationTo(AppInstance targetInstance, AppActivationArguments args)
        {
            // Redirect on a background thread to avoid blocking
            var redirectTask = targetInstance.RedirectActivationToAsync(args).AsTask();
            redirectTask.Wait();
        }

        private static async void OnActivated(object? sender, AppActivationArguments args)
        {
            if (App.Instance is not null)
                await App.Instance.OnActivatedAsync(args);
        }
    }
}
#endif
