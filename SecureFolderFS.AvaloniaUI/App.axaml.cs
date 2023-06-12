using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.AvaloniaUI.ServiceImplementation;
using SecureFolderFS.AvaloniaUI.WindowViews;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Storage.NativeStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI
{
    public sealed partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        public static string AppDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecureFolderFS");

        public App()
        {
            EnsureEarlyApp();
        }

        public override void Initialize()
        {
#if DEBUG
            this.AttachDevTools();
#endif

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Get settings folder
                var settingsFolderPath = Path.Combine(AppDirectory, Constants.LocalSettings.SETTINGS_FOLDER_NAME);
                _ = Directory.CreateDirectory(settingsFolderPath);
                var settingsFolder = new NativeFolder(settingsFolderPath);

                // Configure IoC
                _serviceProvider = ConfigureServices(settingsFolder);
                Ioc.Default.ConfigureServices(_serviceProvider);

                // Activate MainWindow
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void EnsureEarlyApp()
        {
            // Configure exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private IServiceProvider ConfigureServices(IModifiableFolder settingsFolder)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection

                // Singleton services
                .AddSingleton<ISettingsService, SettingsService>(_ => new SettingsService(settingsFolder))
                .AddSingleton<IVaultPersistenceService, VaultPersistenceService>(_ => new VaultPersistenceService(settingsFolder))
                .AddSingleton<IVaultService, VaultService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<IStorageService, NativeStorageService>()
                .AddSingleton<IApplicationService, ApplicationService>()
                .AddSingleton<ILocalizationService, LocalizationService>()
                .AddSingleton<IFileExplorerService, FileExplorerService>()

                // Conditional (singleton) services
#if DEBUG
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#else
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, AvaloniaUpdateService>()
                .AddSingleton<ITelemetryService, TelemetryService>()
#endif

                // Transient services
                .AddTransient<INavigationService, AvaloniaNavigationService>()
                .AddTransient<IPasswordChangeService, PasswordChangeService>()
                .AddTransient<IVaultUnlockingService, VaultUnlockingService>()
                .AddTransient<IVaultCreationService, VaultCreationService>()

                ; // Finish service initialization

            return serviceCollection.BuildServiceProvider();
        }

        #region Exception Handlers

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => LogException(e.ExceptionObject as Exception);

        private static void LogException(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);

            Debug.WriteLine(formattedException);
            Debugger.Break(); // Please check "Output Window" for exception details (On Visual Studio, View -> Output Window or Ctr+Alt+O)

#if !DEBUG
            ExceptionHelpers.LogExceptionToFile(AppDirectory, formattedException);
#endif
        }

        #endregion
    }
}