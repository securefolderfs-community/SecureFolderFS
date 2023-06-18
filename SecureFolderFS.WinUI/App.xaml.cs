using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Storage.NativeStorage;
using SecureFolderFS.WinUI.ServiceImplementation;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.UI.Api;
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            EnsureEarlyApp();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Get settings folder
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, SecureFolderFS.UI.Constants.LocalSettings.SETTINGS_FOLDER_NAME);
            _ = Directory.CreateDirectory(settingsFolderPath);
            var settingsFolder = new NativeFolder(settingsFolderPath);

            // Configure IoC
            _serviceProvider = ConfigureServices(settingsFolder);
            Ioc.Default.ConfigureServices(_serviceProvider);

            // Activate MainWindow
            var window = MainWindow.Instance;
            window.Activate();
        }

        private void EnsureEarlyApp()
        {
            // Configure exception handlers
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if !DEBUG
            // Start AppCenter
            var appCenterKey = ApiKeys.GetAppCenterKey();
            if (!string.IsNullOrEmpty(appCenterKey))
                AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
#endif
        }

        private IServiceProvider ConfigureServices(IModifiableFolder settingsFolder)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection

                // Singleton services
                .AddSingleton<ISettingsService, SettingsService>(_ => new(settingsFolder))
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new(settingsFolder))
                .AddSingleton<IVaultService, VaultService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<IStorageService, NativeStorageService>()
                .AddSingleton<IApplicationService, ApplicationService>()
                .AddSingleton<ILocalizationService, LocalizationService>()
                .AddSingleton<IFileExplorerService, FileExplorerService>()
                .AddSingleton<IChangelogService, GitHubChangelogService>()

                // Conditional (singleton) services
#if DEBUG
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#else
                .AddSingleton<IIapService, DebugIapService>() // .AddSingleton<IIapService, MicrosoftStoreIapService>() // TODO: Change in the future
                .AddSingleton<IUpdateService, MicrosoftStoreUpdateService>()
                .AddSingleton<ITelemetryService, AppCenterTelemetryService>()
#endif

                // Transient services
                .AddTransient<INavigationService, WindowsNavigationService>()
                .AddTransient<IPasswordChangeService, PasswordChangeService>()
                .AddTransient<IVaultUnlockingService, VaultUnlockingService>()
                .AddTransient<IVaultCreationService, VaultCreationService>()
                
                ; // Finish service initialization

            return serviceCollection.BuildServiceProvider();
        }

        #region Exception Handlers

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => LogException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e) => LogException(e.ExceptionObject as Exception);

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
