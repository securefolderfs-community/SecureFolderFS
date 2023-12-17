using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Storage.NativeStorage;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Uno.UI;
using Windows.Storage;
using SecureFolderFS.Uno.ServiceImplementation;

#if !UNPACKAGED
//using Windows.Storage;
#endif

#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.UI.Api;
#endif

namespace SecureFolderFS.Uno
{
    public class App : Application
    {
        public static App? Instance { get; private set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public Window? MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Instance = this;
            EnsureEarlyApp();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
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

#if UNPACKAGED
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#else
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#endif

            // Get settings folder
            var settingsFolder = new NativeFolder(Directory.CreateDirectory(settingsFolderPath));

            // Configure IoC
            ServiceProvider = ConfigureServices(settingsFolder);
            Ioc.Default.ConfigureServices(ServiceProvider);

            // Activate MainWindow
            MainWindow.Content = new MainWindowRootControl();
            MainWindow.Activate();
        }

        private void EnsureEarlyApp()
        {
            // Configure exception handlers
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if !DEBUG
            try
            {
                // Start AppCenter
                var appCenterKey = ApiKeys.GetAppCenterKey();
                if (!string.IsNullOrEmpty(appCenterKey) || !AppCenter.Configured)
                    AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
            }
            catch (Exception)
            {
            }
#endif
        }

        private IServiceProvider ConfigureServices(IModifiableFolder settingsFolder)
        {
            var serviceCollection = new ServiceCollection()

                // Singleton services
                .AddSingleton<ISettingsService, SettingsService>(_ => new(settingsFolder))
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                .AddSingleton<IVaultService, VaultService>()
                .AddSingleton<IDialogService, DialogService>()
                //.AddSingleton<IPrinterService, WindowsPrinterService>()
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<IStorageService, NativeStorageService>()
                .AddSingleton<IApplicationService, ApplicationService>()
                .AddSingleton<IFileExplorerService, FileExplorerService>()
                .AddSingleton<IChangelogService, GitHubChangelogService>()
                .AddSingleton<IVaultManagerService, WindowsVaultManagerService>()

                // Transient services
                .AddTransient<INavigationService, WindowsNavigationService>()

                // ILocalizationService
#if UNPACKAGED
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
#else
                .AddSingleton<ILocalizationService, PackageLocalizationService>()
#endif

                // IIApService, IUpdateService
#if DEBUG || UNPACKAGED
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
#else
                .AddSingleton<IIapService, DebugIapService>() // .AddSingleton<IIapService, MicrosoftStoreIapService>() // TODO: Change in the future
                .AddSingleton<IUpdateService, MicrosoftStoreUpdateService>()
#endif

                // ITelemetryService
#if DEBUG
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#else
                .AddSingleton<ITelemetryService, AppCenterTelemetryService>()
#endif

                ; // Finish service initialization

            return serviceCollection.BuildServiceProvider();
        }

        #region Exception Handlers

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => LogException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e) => LogException(e.ExceptionObject as Exception);

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private static void LogException(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);

            Debug.WriteLine(formattedException);
            Debugger.Break(); // Please check "Output Window" for exception details (On Visual Studio, View -> Output Window or Ctr+Alt+O)

#if !DEBUG
            ExceptionHelpers.LogExceptionToFile(ApplicationData.Current.LocalFolder.Path, formattedException);
#endif
        }

        #endregion
    }
}
