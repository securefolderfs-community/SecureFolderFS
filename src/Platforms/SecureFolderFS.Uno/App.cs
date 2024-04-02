using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Uno.UI;

namespace SecureFolderFS.Uno
{
    public abstract class App : Application
    {
        public static App? Instance { get; private set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public Window? MainWindow { get; private set; }

        protected abstract BaseLifecycleHelper ApplicationLifecycle { get; }

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        protected App()
        {
            Instance = this;

            // Configure exception handlers
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Configures provided <paramref name="window"/> instance for use.
        /// </summary>
        /// <param name="window">The window to configure.</param>
        protected virtual void EnsureEarlyWindow(Window window)
        {
            window.Content = new MainWindowRootControl();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
#if NET6_0_OR_GREATER && WINDOWS && !HAS_UNO
            MainWindow = new Window();
#else
            MainWindow = Microsoft.UI.Xaml.Window.Current;
#endif

#if DEBUG
            MainWindow.EnableHotReload();
#endif

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (MainWindow.Content is not null)
            {
                MainWindow.Activate();
                return;
            }

            // Initialize application lifecycle
            await ApplicationLifecycle.InitAsync();

            // Configure IoC
            ServiceProvider = ApplicationLifecycle.ServiceCollection.BuildServiceProvider();

            // Register IoC
            Ioc.Default.ConfigureServices(ServiceProvider);

            // Activate MainWindow
            EnsureEarlyWindow(MainWindow);
            MainWindow.Activate();
        }

        #region Exception Handlers

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.ExceptionObject as Exception);

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        #endregion
    }
}
