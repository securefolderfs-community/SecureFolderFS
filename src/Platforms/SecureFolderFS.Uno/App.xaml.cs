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
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Uno.UI;
using Windows.ApplicationModel;
using H.NotifyIcon;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Uno
{
    public partial class App : Application
    {
        public static App? Instance { get; private set; }

        public bool UseForceClose { get; set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public Window? MainWindow { get; private set; }

        public BaseLifecycleHelper ApplicationLifecycle { get; } =
#if WINDOWS
            new Platforms.Windows.Helpers.WindowsLifecycleHelper();
#elif MACCATALYST || __MACOS__
            new Platforms.MacCatalyst.Helpers.MacOsLifecycleHelper();
#elif HAS_UNO_SKIA
            new Platforms.Desktop.Helpers.SkiaLifecycleHelper();
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

            // Initialize Telemetry
            var telemetryService = DI.Service<ITelemetryService>();
            await telemetryService.EnableTelemetryAsync();

            // Prepare MainWindow
            EnsureEarlyWindow(MainWindow);

            // Activate MainWindow
            MainWindow.Activate();
        }

        #region Window Configuration

        private static void EnsureEarlyWindow(Window window)
        {
            // Set window content
            window.Content = new MainWindowRootControl();

            // Attach event for window closing
            window.Closed += Window_Closed;

#if WINDOWS
            var appWindow = window.AppWindow;

#if !UNPACKAGED
            // Set icon
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, UI.Constants.FileNames.ICON_ASSET_PATH));
#endif
            // Set backdrop
            window.SystemBackdrop = new MicaBackdrop();

            // Set title
            appWindow.Title = nameof(SecureFolderFS);

            if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                // Extend title bar
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                // Set window buttons background to transparent
                appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else if (window.Content is MainWindowRootControl rootControl)
            {
                window.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(rootControl.CustomTitleBar);
            }

            // Get BoundsManager
            var boundsManager = Platforms.Windows.Helpers.WindowsBoundsManager.AddOrGet(window);

            // Set minimum window size
            boundsManager.MinWidth = 662;
            boundsManager.MinHeight = 572;

            // Load saved window state
            if (!boundsManager.LoadWindowState(UI.Constants.MAIN_WINDOW_ID))
                window.AppWindow.MoveAndResize(new(100, 100, 1050, 680));

#else
            window.Title = nameof(SecureFolderFS);
            global::Uno.Resizetizer.WindowExtensions.SetWindowIcon(window);
#endif
        }

        private static async void Window_Closed(object sender, WindowEventArgs args)
        {
#if WINDOWS
            if (App.Instance?.MainWindow is { } mainWindow)
            {
                var boundsManager = Platforms.Windows.Helpers.WindowsBoundsManager.AddOrGet(mainWindow);
                boundsManager.SaveWindowState(UI.Constants.MAIN_WINDOW_ID);
            }
#endif
            var settingsService = DI.Service<ISettingsService>();
            var useForceClose = App.Instance!.UseForceClose;
            var reduceToBackground = settingsService.UserSettings.ReduceToBackground;

            if (reduceToBackground && !useForceClose)
            {
                args.Handled = true;
                App.Instance.MainWindow?.Hide(enableEfficiencyMode: false);
            }
            else
            {
                await SafetyHelpers.NoFailureAsync(async () => await settingsService.TrySaveAsync());
                SafetyHelpers.NoFailure(static () => FileSystemManager.Instance.FileSystems.DisposeAll());
                Application.Current.Exit();
            }
        }

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
