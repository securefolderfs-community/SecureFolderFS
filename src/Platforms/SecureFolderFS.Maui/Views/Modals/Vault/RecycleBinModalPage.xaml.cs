using MauiIcons.Core;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace SecureFolderFS.Maui.Views.Modals.Vault
{
    public partial class RecycleBinModalPage : BaseModalPage, IOverlayControl
    {
        private PickerOptionViewModel? _previousOption;
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;
        private readonly FirstTimeHelper _firstTime;

        public RecycleBinModalPage(INavigation sourceNavigation)
        {
            _sourceNavigation = sourceNavigation;
            _modalTcs = new();
            _firstTime = new(2);
            BindingContext = this;

            _ = new MauiIcon(); // Workaround for XFC0000
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            // Using Shell to display modals is broken - each new page shown after a 'modal' page will be incorrectly displayed as another modal.
            // NavigationPage approach does not have this issue
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
            ViewModel = viewable as RecycleBinOverlayViewModel;
            OnPropertyChanged(nameof(ViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
        }

        private async void Switch_Toggled(object? sender, ToggledEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (_firstTime.IsFirstTime() && ViewModel.IsRecycleBinEnabled)
                return;

            if (sender is not Switch toggleSwitch)
                return;

            if (!toggleSwitch.IsEnabled)
                return;

            try
            {
                toggleSwitch.IsEnabled = false;
                await ViewModel.ToggleRecycleBinAsync(e.Value);
                toggleSwitch.IsToggled = ViewModel.IsRecycleBinEnabled;
            }
            finally
            {
                toggleSwitch.IsEnabled = true;
            }
        }

        private async void Picker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;

            _previousOption ??= ViewModel.CurrentSizeOption;
            if (_firstTime.IsFirstTime())
                return;

            await ViewModel.ToggleRecycleBinAsync(ViewModel.IsRecycleBinEnabled);
            await ViewModel.UpdateSizesAsync(_previousOption is null || _previousOption.Id == "-1");
            _previousOption = ViewModel.CurrentSizeOption;
        }

        private void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not View { BindingContext: SelectableItemViewModel itemViewModel } )
                return;

            if (ViewModel?.IsSelecting ?? false)
                itemViewModel.IsSelected = !itemViewModel.IsSelected;
        }
        
        private void ProgressTrack_OnSizeChanged(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;
            
            // Nudge the binding to re-evaluate with the now-known width
            var current = ViewModel.PercentageTaken;
            ViewModel.PercentageTaken = -1;
            ViewModel.PercentageTaken = current;
        }

        public RecycleBinOverlayViewModel? ViewModel
        {
            get => (RecycleBinOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(RecycleBinOverlayViewModel), typeof(RecycleBinModalPage));
    }
}
