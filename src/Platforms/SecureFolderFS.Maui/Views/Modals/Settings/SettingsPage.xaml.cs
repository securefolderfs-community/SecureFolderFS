using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Enums;
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
        private readonly FirstTimeHelper _firstTime = new(1);

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
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(50);
            ThemePicker.Items.Clear();
            ThemePicker.Items.Add("ThemeSystemDefault".ToLocalized());
            ThemePicker.Items.Add("ThemeLight".ToLocalized());
            ThemePicker.Items.Add("ThemeDark".ToLocalized());
            ThemePicker.SelectedIndex = (int)MauiThemeHelper.Instance.ActualTheme;
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            _modalTcs.TrySetResult(Result.Success);
            base.OnDisappearing();
        }

        [RelayCommand]
        private async Task ShowIntroductionAsync()
        {
            if (AboutViewModel is null)
                return;
            
            await _sourceNavigation.PopAsync();
            await Task.Delay(500);
            await AboutViewModel.OpenOnboardingCommand.ExecuteAsync(null);
        }

        private async void ThemePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_firstTime.IsFirstTime())
                return;

            await MauiThemeHelper.Instance.SetThemeAsync((ThemeType)ThemePicker.SelectedIndex);
        }
    }
}

