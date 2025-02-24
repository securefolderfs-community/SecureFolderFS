using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Maui.Mappers;
using SecureFolderFS.Shared;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui
{
    public partial class App : Application
    {
        public static App Instance => (App)Application.Current!;

        public IServiceProvider? ServiceProvider { get; private set; }

        public BaseLifecycleHelper ApplicationLifecycle { get; } =
#if ANDROID
            new Platforms.Android.Helpers.AndroidLifecycleHelper();
#elif IOS
            new Platforms.iOS.Helpers.IOSLifecycleHelper();
#else
            null;
#endif

        public event EventHandler? AppResumed;
        public event EventHandler? AppPutToForeground;

        public App()
        {
            InitializeComponent();
            
            // Configure mappers
            CustomMappers.AddEntryMappers();

            // Configure exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            APES.UI.XF.ContextMenuContainer.Init();
            
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
            
            // Create and initialize AppShell
            var appShell = new AppShell();
            await appShell.MainViewModel.InitAsync();

            return appShell;
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
