using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using SecureFolderFS.Uno.Windows.Helpers;
using Windows.ApplicationModel;

namespace SecureFolderFS.Uno
{
    public sealed partial class AppHead : App
    {
        static AppHead() => InitializeLogging();

        /// <inheritdoc/>
        protected override BaseLifecycleHelper ApplicationLifecycle { get; } =
#if WINDOWS
            new WindowsLifecycleHelper()
#else
            null
#endif
            ;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public AppHead()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void EnsureEarlyWindow(Window window)
        {
            base.EnsureEarlyWindow(window);

            // Set title
            window.AppWindow.Title = "SecureFolderFS";

#if WINDOWS
#if !UNPACKAGED
            // Set icon
            window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, UI.Constants.FileNames.ICON_ASSET_PATH));
#endif
            // Set backdrop
            window.SystemBackdrop = new MicaBackdrop();

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                // Extend title bar
                window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                // Set window buttons background to transparent
                window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
#endif
            if (window.Content is MainWindowRootControl rootControl)
            {
                window.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(rootControl.CustomTitleBar);
            }
            
            // Hook up event for window closing
            window.AppWindow.Closing += AppWindow_Closing;
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            await settingsService.TrySaveAsync();
        }

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
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
#elif NETFX_CORE
                builder.AddDebug();
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
            //global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
        }
    }
}
