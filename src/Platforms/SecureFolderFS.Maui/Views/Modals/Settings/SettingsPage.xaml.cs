using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace SecureFolderFS.Maui.Views.Modals.Settings
{
    public partial class SettingsPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;

        public SettingsOverlayViewModel? OverlayViewModel { get; private set; }

        public GeneralSettingsViewModel? GeneralViewModel { get; private set; }

        public PreferencesSettingsViewModel? PreferencesViewModel { get; private set; }

        public PrivacySettingsViewModel? PrivacyViewModel { get; private set; }

        public AboutSettingsViewModel? AboutViewModel { get; private set; }

        public SettingsPage(INavigation sourceNavigation)
        {
            _modalTcs = new();
            _sourceNavigation = sourceNavigation;
            BindingContext = this;
            
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
#if ANDROID
            await _sourceNavigation.PushModalAsync(new NavigationPage(this)
            {
                BackgroundColor = Color.FromArgb("#80000000")
            });
#elif IOS
            var navigationPage = new NavigationPage(this);
            NavigationPage.SetIconColor(navigationPage, Color.FromArgb("#007bff"));
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
            await _sourceNavigation.PushModalAsync(navigationPage);
#endif

            return await _modalTcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            OverlayViewModel = (SettingsOverlayViewModel)viewable;
            GeneralViewModel = OverlayViewModel.NavigationService.Views.GetOrAdd(() => new GeneralSettingsViewModel().WithInitAsync());
            PreferencesViewModel = OverlayViewModel.NavigationService.Views.GetOrAdd(() => new PreferencesSettingsViewModel().WithInitAsync());
            PrivacyViewModel = OverlayViewModel.NavigationService.Views.GetOrAdd(() => new PrivacySettingsViewModel().WithInitAsync());
            AboutViewModel = OverlayViewModel.NavigationService.Views.GetOrAdd(() => new AboutSettingsViewModel().WithInitAsync());
            
            OnPropertyChanged(nameof(OverlayViewModel));
            OnPropertyChanged(nameof(GeneralViewModel));
            OnPropertyChanged(nameof(PreferencesViewModel));
            OnPropertyChanged(nameof(PrivacyViewModel));
            OnPropertyChanged(nameof(AboutViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            _modalTcs.TrySetResult(Result.Success);
            base.OnDisappearing();
        }
    }
}

