using System.Globalization;
using APES.UI.XF;
using SecureFolderFS.Maui.Extensions.Mappers;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.Platforms.iOS.Helpers;
using SecureFolderFS.Maui.Platforms.iOS.Templates;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui
{
    public partial class App : Application
    {
        public static App Instance => (App)Current!;

        public MainViewModel MainViewModel { get; } = new(new VaultCollectionModel());

        public IServiceProvider? ServiceProvider { get; private set; }

        public BaseLifecycleHelper ApplicationLifecycle { get; } =
#if ANDROID
            new Platforms.Android.Helpers.AndroidLifecycleHelper();
#elif IOS
            new IOSLifecycleHelper();
#else
            null;
#endif

        public event EventHandler? AppResumed;
        public event EventHandler? AppPutToForeground;

        public App()
        {
            InitializeComponent();

#if ANDROID
            // Load Android-specific resource dictionaries
            Resources.MergedDictionaries.Add(new Platforms.Android.Templates.AndroidDataTemplates());
#elif IOS
            // Load IOS-specific resource dictionaries
            Resources.MergedDictionaries.Add(new IOSDataTemplates());
#endif

            // Configure mappers
            CustomMappers.AddEntryMappers();
            CustomMappers.AddLabelMappers();
            CustomMappers.AddPickerMappers();

            // Configure exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            ContextMenuContainer.Init();

            var appShell = Task.Run(GetAppShellAsync).ConfigureAwait(false).GetAwaiter().GetResult();
            return new Window(appShell);
        }

        private async Task<Page> GetAppShellAsync()
        {
            // Initialize application lifecycle
            await ApplicationLifecycle.InitAsync();

            // Configure IoC
            ServiceProvider = ApplicationLifecycle.ServiceCollection.BuildServiceProvider();

            // Register IoC
            DI.Default.SetServiceProvider(ServiceProvider);
            
            // Determine app language
            await SafetyHelpers.NoFailureAsync(async () =>
            {
                if (!Preferences.Default.ContainsKey("IsAppLanguageDetected"))
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
                    
                    Preferences.Default.Set("IsAppLanguageDetected", true);
                }
            });

            // Initialize Telemetry
            var telemetryService = DI.Service<ITelemetryService>();
            await telemetryService.EnableTelemetryAsync();
            
            // Initialize MainViewModel
            await MainViewModel.InitAsync();
            
            // Initialize ThemeHelper
            await MauiThemeHelper.Instance.InitAsync().ConfigureAwait(false);

            // Create new AppShell
            return new AppShell();
        }

        /// <inheritdoc/>
        protected override void OnSleep()
        {
            AppPutToForeground?.Invoke(this, EventArgs.Empty);
            base.OnSleep();
        }

        /// <inheritdoc/>
        protected override void OnResume()
        {
            AppResumed?.Invoke(this, EventArgs.Empty);
            base.OnResume();
        }

        #region Exception Handlers

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.ExceptionObject as Exception);

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        #endregion
    }
}
