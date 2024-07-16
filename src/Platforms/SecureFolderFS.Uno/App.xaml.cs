using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Uno.UI;
using Windows.ApplicationModel;

namespace SecureFolderFS.Uno
{
    public partial class App : Application
    {
        public static App? Instance { get; private set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public Window? MainWindow { get; private set; }

        public BaseLifecycleHelper ApplicationLifecycle { get; } =
#if WINDOWS
            new Platforms.Windows.Helpers.WindowsLifecycleHelper();
#elif HAS_UNO_SKIA
            new Platforms.SkiaGtk.Helpers.SkiaLifecycleHelper();
#else
            true ? throw new PlatformNotSupportedException() : null;
#endif


        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Instance = this;
            InitializeComponent();

            // Configure exception handlers
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new Window();
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
            DI.Default.SetServiceProvider(ServiceProvider);

            // Prepare MainWindow
            EnsureEarlyWindow(MainWindow);

            // Activate MainWindow
            MainWindow.Activate();
        }

        #region Window Configuration

        private static void EnsureEarlyWindow(Window window)
        {
            window.Content = new MainWindowRootControl();

#if WINDOWS
#if !UNPACKAGED
            // Set icon
            window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, UI.Constants.FileNames.ICON_ASSET_PATH));
#endif
            // Set backdrop
            window.SystemBackdrop = new MicaBackdrop();

            // Set title
            window.AppWindow.Title = "SecureFolderFS";

            // Attach event for window closing
            window.AppWindow.Closing += AppWindow_Closing;

            if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                // Extend title bar
                window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                // Set window buttons background to transparent
                window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else if (window.Content is MainWindowRootControl rootControl)
            {
                window.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(rootControl.CustomTitleBar);
            }

#else
            _ = window;
#endif
        }

#if WINDOWS
        private static async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            var settingsService = SecureFolderFS.Shared.DI.Service<ISettingsService>();
            await settingsService.TrySaveAsync();
        }
#endif

        #endregion

        #region Logging

        /// <summary>
        /// Configures global Uno Platform logging.
        /// </summary>
        public static void InitializeLogging()
        {
#if DEBUG
            // Logging is disabled by default for release builds, as it incurs a significant
            // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
            // is a concern for your application, keep this disabled. If you're running on the web or
            // desktop targets, you can use URL or command line parameters to enable it.
            //
            // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__ || __MACCATALYST__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // DevServer and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
            global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
        }

        #endregion

        #region Exception Handlers

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.ExceptionObject as Exception);

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        #endregion
    }
}
