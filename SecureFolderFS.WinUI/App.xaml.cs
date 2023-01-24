using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.WinUI.AppModels;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.ServiceImplementation;
using SecureFolderFS.WinUI.ServiceImplementation.UserPreferences;
using SecureFolderFS.WinUI.Storage.NativeStorage;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private Window? _window;

        private IServiceProvider? ServiceProvider { get; set; }

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
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDER_NAME);
            _ = Directory.CreateDirectory(settingsFolderPath);
            var settingsFolder = new NativeFolder(settingsFolderPath);

            // Configure IoC
            ServiceProvider = ConfigureServices(settingsFolder);
            Ioc.Default.ConfigureServices(ServiceProvider);

            _window = new MainWindow();
            _window.Activate();
        }

        private void EnsureEarlyApp()
        {
            // Configure exception handlers
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Start AppCenter
            // TODO: Start AppCenter
        }

        private IServiceProvider ConfigureServices(IModifiableFolder settingsFolder)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton<ISettingsService, SettingsService>(_ => new SettingsService(settingsFolder))
                .AddSingleton<ISavedVaultsService, SavedVaultsService>(_ => new SavedVaultsService(settingsFolder))
                .AddSingleton<IVaultsSettingsService, VaultsSettingsService>(_ => new VaultsSettingsService(settingsFolder))
                .AddSingleton<IVaultsWidgetsService, VaultsWidgetsService>(_ => new VaultsWidgetsService(settingsFolder))
                .AddSingleton<IApplicationSettingsService, ApplicationSettingsService>(_ => new ApplicationSettingsService(settingsFolder))
                .AddSingleton<IGeneralSettingsService, GeneralSettingsService>(sp => GetSettingsService(sp, (database, model) => new GeneralSettingsService(database, model)))
                .AddSingleton<IPreferencesSettingsService, PreferencesSettingsService>(sp => GetSettingsService(sp, (database, model) => new PreferencesSettingsService(database, model)))
                .AddSingleton<IPrivacySettingsService, PrivacySettingsService>(sp => GetSettingsService(sp, (database, model) => new PrivacySettingsService(database, model)))

                .AddTransient<IVaultUnlockingService, VaultUnlockingService>()
                .AddTransient<IVaultCreationService, VaultCreationService>()
                .AddSingleton<IVaultService, VaultService>()
                .AddSingleton<IStorageService, NativeStorageService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<IApplicationService, ApplicationService>()
                .AddSingleton<IThreadingService, ThreadingService>()
                .AddSingleton<ILocalizationService, LocalizationService>()
                .AddSingleton<IFileExplorerService, FileExplorerService>()
                .AddSingleton<IClipboardService, ClipboardService>()
                .AddSingleton<IUpdateService, MicrosoftStoreUpdateService>();

            return serviceCollection.BuildServiceProvider();
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
        }

        private static void LogException(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);

            Debug.WriteLine(formattedException);
            Debugger.Break(); // Please check "Output Window" for exception details (On Visual Studio, View -> Output Window or Ctr+Alt+O)

#if !DEBUG
            ExceptionHelpers.LogExceptionToFile(ApplicationData.Current.LocalFolder.Path, formattedException);
#endif
        }

        // Terrible.
        private static TSettingsService GetSettingsService<TSettingsService>(IServiceProvider serviceProvider,
            Func<IDatabaseModel<string>, ISettingsModel, TSettingsService> initializer) where TSettingsService : SharedSettingsModel
        {
            var settingsServiceImpl = serviceProvider.GetRequiredService<ISettingsService>() as SettingsService;
            return initializer(settingsServiceImpl!.GetDatabaseModel(), settingsServiceImpl);
        }
    }
}
