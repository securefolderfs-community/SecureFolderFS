using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        public BaseLifecycleHelper ApplicationLifecycle { get; } =
#if ANDROID
            new Platforms.Android.Helpers.AndroidLifecycleHelper();
#else
            null;
#endif

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();

            // Configure exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <inheritdoc/>
        protected override async void OnStart()
        {
            // Initialize application lifecycle
            await ApplicationLifecycle.InitAsync();

            // Configure IoC
            ServiceProvider = ApplicationLifecycle.ServiceCollection
                .WithMauiServices()
                .BuildServiceProvider();

            // Register IoC
            Ioc.Default.ConfigureServices(ServiceProvider);
            base.OnStart();
        }

        #region Exception Handlers

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => ApplicationLifecycle.LogException(e.ExceptionObject as Exception);

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => ApplicationLifecycle.LogException(e.Exception);

        #endregion
    }
}
