using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog, IOverlayControl
    {
        private bool _firstNavigated;

        public SettingsOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<SettingsOverlayViewModel>();
            set => DataContext = value;
        }

        public SettingsDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (SettingsOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private IViewDesignation GetViewForTag(int tag)
        {
            return tag switch
            {
                0 => ViewModel?.NavigationService.TryGetView<GeneralSettingsViewModel>() ?? new(),
                1 => ViewModel?.NavigationService.TryGetView<PreferencesSettingsViewModel>() ?? new(),
                2 => ViewModel?.NavigationService.TryGetView<PrivacySettingsViewModel>() ?? new(),
                3 => ViewModel?.NavigationService.TryGetView<AboutSettingsViewModel>() ?? new(),
                _ => new GeneralSettingsViewModel()
            };
        }

        private async Task NavigateToTagAsync(int tag)
        {
            if (ViewModel is null || (!ViewModel?.NavigationService.SetupNavigation(Navigation) ?? true))
                return;

            _firstNavigated = true;
            var target = GetViewForTag(tag);
            if (ViewModel.NavigationService.Views.FirstOrDefault(x => target == x) is null && target is IAsyncInitialize asyncInitialize)
                _ = asyncInitialize.InitAsync();

            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = Convert.ToInt32((args.SelectedItem as NavigationViewItem)?.Tag);
            await NavigateToTagAsync(tag);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SettingsDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Remove the reference to the NavigationControl so the dialog can get properly garbage collected
            ViewModel?.NavigationService.ResetNavigation();
        }

        private async void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_firstNavigated)
                await NavigateToTagAsync(0);
        }
    }
}
