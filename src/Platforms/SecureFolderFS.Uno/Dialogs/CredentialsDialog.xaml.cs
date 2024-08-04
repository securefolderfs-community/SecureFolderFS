using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class CredentialsDialog : ContentDialog, IOverlayControl
    {
        private bool _isBackShown;

        public CredentialsOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<CredentialsOverlayViewModel>();
            set => DataContext = value;
        }

        public CredentialsDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (CredentialsOverlayViewModel)viewable;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.OnAppearing();
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private async Task AnimateBackAsync(bool shouldShowBack)
        {
            if (shouldShowBack && !_isBackShown)
            {
                _isBackShown = true;
                await BackTitle.ShowBackAsync();
            }
            else if (!shouldShowBack && _isBackShown)
            {
                _isBackShown = false;
                await BackTitle.HideBackAsync();
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

            args.Cancel = true;
            if (ViewModel.SelectedViewModel is LoginViewModel loginViewModel)
                loginViewModel.ProvideCredentialsCommand?.Execute(null);
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            ViewModel?.OnDisappearing();
        }

        private void RecoverLink_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void ActionBlockControl_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (sender is not FrameworkElement { DataContext: AuthenticationViewModel selectedAuthentication })
                return;

            await ViewModel.SelectionViewModel.ItemSelectedCommand.ExecuteAsync(selectedAuthentication);
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (ViewModel.SelectedViewModel is not CredentialsConfirmationViewModel)
                return;

            ViewModel.SelectedViewModel = ViewModel.SelectionViewModel;
            ViewModel.PrimaryButtonText = null;
            ViewModel.Title = "Select authentication option";

            await AnimateBackAsync(false);
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e.PropertyName == nameof(CredentialsOverlayViewModel.SelectedViewModel))
            {
                if (ViewModel.SelectedViewModel is CredentialsConfirmationViewModel)
                    await AnimateBackAsync(true);
            }
        }

        private void CredentialsDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (ViewModel is not null)
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
}
