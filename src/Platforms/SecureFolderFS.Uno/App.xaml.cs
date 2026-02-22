using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Uno.UI;
using Windows.ApplicationModel;
using Windows.Storage;
using Microsoft.UI.Windowing;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace SecureFolderFS.Uno
{
    public partial class App : Application
    {
        public static App? Instance { get; private set; }

        public bool UseForceClose { get; set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public Window? MainWindow { get; private set; }

        public SynchronizationContext? MainWindowSynchronizationContext { get; set; }

        public MainViewModel? MainViewModel { get; private set; }

        /// <summary>
        /// Gets a task that completes when the main window has finished initializing.
        /// </summary>
        public TaskCompletionSource MainWindowInitialized { get; } = new();

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
            MainWindow ??= new Window();
#if DEBUG
            MainWindow.UseStudio();
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
            
            // Determine app language
            await SafetyHelpers.NoFailureAsync(async () =>
            {
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("IsAppLanguageDetected"))
                {
                    // Check the current system language and find it in AppLanguages
                    // If it doesn't exist, use en-US
                    var localizationService = DI.Service<ILocalizationService>();
                    var systemCulture = CultureInfo.CurrentUICulture;
                    var appLanguages = localizationService.AppLanguages;
                    var matchedLanguage = appLanguages.FirstOrDefault(lang => lang.Name.Equals(systemCulture.Name, StringComparison.OrdinalIgnoreCase))
                                          ?? appLanguages.FirstOrDefault(lang => lang.TwoLetterISOLanguageName.Equals(systemCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                                          ?? appLanguages.FirstOrDefault(lang => lang.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase));

                    if (matchedLanguage is not null)
                        await localizationService.SetCultureAsync(matchedLanguage);
                    
                    ApplicationData.Current.LocalSettings.Values["IsAppLanguageDetected"] = true;
                }
            });

            // Initialize Telemetry
            var telemetryService = DI.Service<ITelemetryService>();
            await telemetryService.EnableTelemetryAsync();

            // Initialize MainViewModel
            MainViewModel = new(new VaultCollectionModel());

            // Prepare MainWindow
            EnsureMainWindow(MainWindow, MainViewModel);

#if WINDOWS
            // Check if the app was launched via file activation (shortcut file)
            var isShortcutActivation = IsShortcutFileActivation(Program.InitialActivationArgs);

            // Activate MainWindow (required for initialization)
            MainWindow.Activate();

            // If launched via shortcut file, hide the main window immediately
            if (isShortcutActivation)
                MainWindow.Hide(enableEfficiencyMode: false);

            // Process initial file activation if the app was launched via file association
            if (Program.InitialActivationArgs is { } initialArgs)
                await OnActivatedAsync(initialArgs);
#else
            // Activate MainWindow
            MainWindow.Activate();
#endif
        }

#if WINDOWS
        /// <summary>
        /// Checks if the activation arguments represent a vault shortcut file activation.
        /// </summary>
        private static bool IsShortcutFileActivation(AppActivationArguments? args)
        {
            if (args is null)
                return false;

            if (args.Kind != ExtendedActivationKind.File)
                return false;

            if (args.Data is not Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs)
                return false;

            var file = fileArgs.Files.Count > 0 ? fileArgs.Files[0] : null;
            return file is IStorageFile storageFile && 
                   storageFile.Path.EndsWith(UI.Constants.FileNames.VAULT_SHORTCUT_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }
#endif

        /// <summary>
        /// Invoked when the application is activated by opening a file.
        /// </summary>
        public async Task OnActivatedAsync(AppActivationArguments args)
        {
            if (args.Kind != ExtendedActivationKind.File)
                return;

            if (args.Data is not Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs)
                return;

            var file = fileArgs.Files.Count > 0 ? fileArgs.Files[0] : null;
            if (file is not IStorageFile storageFile || !storageFile.Path.EndsWith(UI.Constants.FileNames.VAULT_SHORTCUT_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                return;

            await HandleVaultShortcutActivationAsync(storageFile.Path);
        }

        /// <summary>
        /// Handles vault shortcut file activation.
        /// </summary>
        /// <param name="filePath">The path to the extension file.</param>
        public async Task HandleVaultShortcutActivationAsync(string filePath)
        {
            var shortcutFile = new SystemFileEx(filePath);
            await using var shortcutStream = await shortcutFile.OpenReadAsync(default);

            var shortcutData = await SerializationExtensions.DeserializeAsync<Stream, VaultShortcutDataModel>(StreamSerializer.Instance, shortcutStream);
            if (shortcutData?.PersistableId is null || MainViewModel is null)
                return;

            // Wait for the main window to finish initializing (vault collection loaded, navigation set up)
            await MainWindowInitialized.Task;

            // Find the vault in the collection by PersistableId
            var listItemViewModel = MainViewModel.VaultListViewModel.Items.FirstOrDefault(x => x.VaultViewModel.VaultModel.DataModel.PersistableId == shortcutData.PersistableId);
            if (listItemViewModel is null)
                return;

            var vaultViewModel = listItemViewModel.VaultViewModel;
            await MainWindowSynchronizationContext.PostOrExecuteAsync(async _ =>
            {
                if (MainViewModel.RootNavigationService.CurrentView is not MainHostViewModel mainHostViewModel)
                    return;

                // Create the preview window
                var window = new Window();
                window.Closed += PreviewWindow_Closed;

                // Initialize preview view model
                var title = $"{nameof(SecureFolderFS)} - {listItemViewModel.VaultViewModel.Title}";
                var vaultPreviewViewModel = !vaultViewModel.IsUnlocked
                    ? new VaultPreviewViewModel(vaultViewModel, mainHostViewModel.NavigationService)
                    : new VaultPreviewViewModel(vaultViewModel, mainHostViewModel.NavigationService)
                    {
                        UnlockedVaultViewModel = vaultViewModel.GetUnlockedViewModel()
                    };

                window.Content = new VaultPreviewRootControl(vaultPreviewViewModel);
                EnsureEarlyWindow(window, title);

#if WINDOWS
                // Get BoundsManager
                var boundsManager = Platforms.Windows.Helpers.WindowsBoundsManager.AddOrGet(window);

                // Set minimum window size
                boundsManager.MinWidth = 464;
                boundsManager.MinHeight = 640;
                window.AppWindow.MoveAndResize(new(100, 100, 464, 640));
#endif

                // Initialize the login view model
                await vaultPreviewViewModel.InitAsync();

                window.Activate();
            });

            static void PreviewWindow_Closed(object sender, WindowEventArgs args)
            {
                if (sender is not Window window)
                    return;

                window.Closed -= PreviewWindow_Closed;
                (window.Content as VaultPreviewRootControl)?.ViewModel?.Dispose();
            }
        }

        #region Window Configuration

        private static void EnsureEarlyWindow(Window window, string title)
        {
            var appWindow = window.AppWindow;

#if !UNPACKAGED
            // Set icon
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, UI.Constants.FileNames.ICON_ASSET_PATH));
#endif
#if WINDOWS
            // Set backdrop
            window.SystemBackdrop = new MicaBackdrop();
#endif

            // Set title
            appWindow.Title = title;

            // Extend title bar
            if (window.Content is MainWindowRootControl rootControl)
            {
                window.ExtendsContentIntoTitleBar = true;
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(rootControl.CustomTitleBar);

#if __UNO_SKIA_MACOS__
                // Use native macOS APIs to configure the window
                Platforms.Desktop.Helpers.MacOsTitleBarHelper.ConfigureFullSizeContentView(window);
                Platforms.Desktop.Helpers.MacOsIconHelper.SetDockIcon(Directory.GetCurrentDirectory() + "/Assets/AppIcon/AppIcon.icns");

                // Add left padding for traffic light buttons
                var (leftPadding, _) = Platforms.Desktop.Helpers.MacOsTitleBarHelper.GetTrafficLightButtonsInset();
                rootControl.CustomTitleBar.Margin = new Thickness(leftPadding, 0, 0, 0);
#elif !WINDOWS
                // For other non-Windows platforms, use OverlappedPresenter
                if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
                {
                    overlappedPresenter.SetBorderAndTitleBar(true, false);
                    overlappedPresenter.IsMinimizable = true;
                    overlappedPresenter.IsMaximizable = true;
                }
#endif
            }

#if WINDOWS
            if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                // Set window buttons background to transparent
                appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
#endif
            window.Title = title;
        }

        private static void EnsureMainWindow(Window window, MainViewModel mainViewModel)
        {
            // Set window content
            window.Content = new MainWindowRootControl(mainViewModel);

            // Attach event for window closing
            window.Closed += Window_Closed;

            // Enable early window configuration
            EnsureEarlyWindow(window, nameof(SecureFolderFS));

#if WINDOWS
            // Get BoundsManager
            var boundsManager = Platforms.Windows.Helpers.WindowsBoundsManager.AddOrGet(window);

            // Set minimum window size
            boundsManager.MinWidth = 662;
            boundsManager.MinHeight = 572;

            // Load saved window state
            if (!boundsManager.LoadWindowState(UI.Constants.MAIN_WINDOW_ID))
                window.AppWindow.MoveAndResize(new(100, 100, 1050, 680));
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
