using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui
{
    public partial class AppShell : Shell
    {
        public MainViewModel MainViewModel { get; } = new(new VaultCollectionModel());

        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("OverviewPage", typeof(OverviewPage));
            Routing.RegisterRoute("BrowserPage", typeof(BrowserPage));
            Routing.RegisterRoute("HealthPage", typeof(HealthPage));
        }

        private async void AppShell_Loaded(object? sender, EventArgs e)
        {
            var settingsService = DI.Service<ISettingsService>();
            if (!settingsService.AppSettings.WasIntroduced)
            {
                var overlayService = DI.Service<IOverlayService>();
                await overlayService.ShowAsync(new IntroductionOverlayViewModel().WithInitAsync());
                
                settingsService.AppSettings.WasIntroduced = true;
                await settingsService.AppSettings.TrySaveAsync();
            }
            
            await SafetyHelpers.NoFailureAsync(async () =>
            {
                var sessionException = ExceptionHelpers.RetrieveSessionFile(App.Instance.ApplicationLifecycle.AppDirectory);
                if (sessionException is null)
                    return;

                var overlayService = DI.Service<IOverlayService>();
                var messageOverlay = new MessageOverlayViewModel()
                {
                    Title = "ClosedUnexpectedly".ToLocalized(nameof(SecureFolderFS)),
                    PrimaryText = "Copy".ToLocalized(),
                    SecondaryText = "Close".ToLocalized(),
                    Message = sessionException
                };

                var result = await overlayService.ShowAsync(messageOverlay);
                if (!result.Positive())
                    return;

                var clipboardService = DI.Service<IClipboardService>();
                await clipboardService.SetTextAsync(sessionException);
            });
            
            // Initialize DeviceLink
            await SafetyHelpers.NoFailureAsync(async () => await DeviceLinkCredentialsOverlayViewModel.Instance.InitAsync());
        }
    }
}
